import pandas as pd
import tensorflow as tf
from tensorflow import keras
from tensorflow.keras import layers
from tensorflow.keras.models import Model
from tensorflow.keras.layers import (Conv2D, MaxPooling2D, Dense, UpSampling2D, 
                                     Flatten, Reshape, Lambda)
from tensorflow.keras.optimizers import Adam
import numpy as np
from sklearn.model_selection import train_test_split
from sklearn.utils import class_weight

# -------------------------
# Custom loss functions
# -------------------------
def custom_weighted_binary_crossentropy(zero_weight, one_weight):
    def loss(y_true, y_pred):
        # Flatten the tensors
        y_true = tf.reshape(y_true, [-1])
        y_pred = tf.reshape(y_pred, [-1])
        
        # Clip predicted values to avoid log(0)
        y_pred = tf.clip_by_value(y_pred, 1e-7, 1 - 1e-7)
        y_true = tf.cast(y_true, tf.float32)
        
        # Calculate the binary cross-entropy loss
        bce_loss = y_true * tf.math.log(y_pred) + (1 - y_true) * tf.math.log(1 - y_pred)
        
        # Create a weight tensor based on the true labels
        weights = tf.where(tf.equal(y_true, 1), one_weight, zero_weight)
        
        # Apply the weights to the loss
        weighted_bce_loss = weights * bce_loss
        
        # Return the mean weighted loss
        return -tf.reduce_mean(weighted_bce_loss)
    return loss

def weighted_binary_crossentropy(class_weights):
    class_weights = tf.cast(class_weights, tf.float32)
    def loss(y_true, y_pred):
        y_true = tf.reshape(y_true, [-1])
        y_pred = tf.reshape(y_pred, [-1])
        y_pred = tf.clip_by_value(y_pred, 1e-7, 1 - 1e-7)
        y_true = tf.cast(y_true, tf.float32)
        bce_loss = y_true * tf.math.log(y_pred) + (1 - y_true) * tf.math.log(1 - y_pred)
        class_weights_tensor = tf.gather(class_weights, tf.cast(y_true, tf.int32))
        weighted_bce_loss = class_weights_tensor * bce_loss
        return -tf.reduce_mean(weighted_bce_loss)
    return loss

# -------------------------
# Data loading and preprocessing
# -------------------------
# Read the CSV file
df = pd.read_csv('MoveVAETrainingData.csv')

# Load the data and reshape (assumes image data in the CSV)
X = df.iloc[:, :-3].values
X = X.reshape(X.shape[0], 28, 28, 1)
Y = df.iloc[:, -3:].values

# Split into training / test sets
X_train, X_test, y_train, y_test = train_test_split(X, Y, test_size=0.2)
# (Optional) compute class weights – note: for images the pixels are 0/1 so these might not be critical
class_weights = class_weight.compute_class_weight(None, classes=np.unique(X_train), y=X_train.flatten())

# -------------------------
# VAE model parameters
# -------------------------
intermediate_dim = 64  # This can be any value you choose
latent_dim = 32  # Dimension of the latent space

# -------------------------
# Build the Encoder
# -------------------------
encoder_inputs = keras.Input(shape=(28, 28, 1), name='encoder_input')
x = Conv2D(8, (3, 3), activation='relu', padding='same',
           kernel_regularizer=keras.regularizers.l2(0.001))(encoder_inputs)
x = layers.Dropout(0.2)(x)
x = MaxPooling2D((2, 2), padding='same')(x)
x = Conv2D(8, (3, 3), activation='relu', padding='same',
           kernel_regularizer=keras.regularizers.l2(0.001))(x)
x = layers.Dropout(0.2)(x)
x = MaxPooling2D((2, 2), padding='same')(x)

# Save shape for later (needed in the decoder)
shape_before_flattening = keras.backend.int_shape(x)[1:]  # e.g. (7, 7, 8)
x = Flatten()(x)
x = Dense(intermediate_dim, activation='relu')(x)

# Instead of directly outputting an encoded vector,
# we create two vectors: one for the mean and one for the log variance.
z_mean = Dense(latent_dim, name='z_mean')(x)
z_log_var = Dense(latent_dim, name='z_log_var')(x)

# Reparameterization trick: sample z ~ N(z_mean, exp(z_log_var))
def sampling(args):
    z_mean, z_log_var = args
    batch = tf.shape(z_mean)[0]
    dim = tf.shape(z_mean)[1]
    epsilon = tf.random.normal(shape=(batch, dim))
    return z_mean + tf.exp(0.5 * z_log_var) * epsilon

# Use a Lambda layer to perform the sampling
z = Lambda(sampling, output_shape=(latent_dim,), name='z')([z_mean, z_log_var])

# Create the encoder model which outputs [z_mean, z_log_var, z]
encoder = Model(encoder_inputs, [z_mean, z_log_var, z], name='encoder')

# -------------------------
# Build the Decoder
# -------------------------
latent_inputs = keras.Input(shape=(latent_dim,), name='z_sampling')
# Map the latent vector back to the shape before flattening
x = Dense(np.prod(shape_before_flattening), activation='relu')(latent_inputs)
x = Reshape(shape_before_flattening)(x)
x = Conv2D(8, (3, 3), activation='relu', padding='same',
           kernel_regularizer=keras.regularizers.l2(0.001))(x)
x = layers.Dropout(0.2)(x)
x = UpSampling2D((2, 2))(x)
x = Conv2D(8, (3, 3), activation='relu', padding='same',
           kernel_regularizer=keras.regularizers.l2(0.001))(x)
x = layers.Dropout(0.2)(x)
x = UpSampling2D((2, 2))(x)
decoder_outputs = Conv2D(1, (3, 3), activation='sigmoid', padding='same')(x)
decoder = Model(latent_inputs, decoder_outputs, name='decoder')

# -------------------------
# Build the VAE Model
# -------------------------
# Connect the encoder and decoder
z_mean, z_log_var, z = encoder(encoder_inputs)
vae_outputs = decoder(z)
vae = Model(encoder_inputs, vae_outputs, name='vae')

# -------------------------
# Add the KL divergence loss
# -------------------------
# The KL divergence loss encourages the latent space to follow a standard normal distribution.
kl_loss = -0.5 * tf.reduce_mean(
    tf.reduce_sum(1 + z_log_var - tf.square(z_mean) - tf.exp(z_log_var), axis=1)
)
vae.add_loss(kl_loss)

# -------------------------
# Compile and Train the VAE
# -------------------------
# You can choose either your weighted BCE or custom weighted BCE; here we use custom_weighted_binary_crossentropy.
zero_weight = 0.5  # Example weight for zeros
one_weight = 0.5   # Example weight for ones

vae.compile(optimizer='adam',
            loss=custom_weighted_binary_crossentropy(zero_weight, one_weight),
            metrics=['accuracy'])

vae.fit(X_train, X_train, epochs=20, batch_size=256)

# -------------------------
# Model Information
# -------------------------
print("_______________________________________________________________________")
print("Model Info")
print("_______________________________________________________________________")
print("X shape:", X.shape)
print("Y shape:", Y.shape)
vae.summary()
print("_______________________________________________________________________")
print("Encoder Info")
encoder.summary()
print("_______________________________________________________________________")
print("Decoder Info")
decoder.summary()

# -------------------------
# Reconstruction on Training and Test Data
# -------------------------
# (For a VAE the reconstruction accuracy is only one indicator of performance.)
reconstructed_train = vae.predict(X_train)
threshold = 0.5
reconstructed_train_thresholded = np.where(reconstructed_train >= threshold, 1, 0)
train_accuracy = np.mean(np.equal(reconstructed_train_thresholded, X_train))
print("Reconstruction accuracy on training data:", train_accuracy)

reconstructed_test = vae.predict(X_test)
reconstructed_test_thresholded = np.where(reconstructed_test >= threshold, 1, 0)
test_accuracy = np.mean(np.equal(reconstructed_test_thresholded, X_test))
print("Reconstruction accuracy on test data:", test_accuracy)

# -------------------------
# Save the Models
# -------------------------
tf.saved_model.save(vae, "saved_vae_model_dir")
tf.saved_model.save(encoder, "saved_ve_model_dir")