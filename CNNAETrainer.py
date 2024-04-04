import pandas as pd
import tensorflow as tf
from tensorflow import keras
from tensorflow.keras.models import Sequential, Model
from keras import layers
from tensorflow.keras.layers import Flatten, Conv2D, MaxPooling2D, Dense, UpSampling2D
from tensorflow.keras.optimizers import Adam
import numpy as np
from sklearn.metrics import classification_report
from sklearn.model_selection import train_test_split
from sklearn.utils import class_weight

def custom_weighted_binary_crossentropy(zero_weight, one_weight):
    def loss(y_true, y_pred):
        # Flatten the tensors
        y_true = tf.reshape(y_true, [-1])
        y_pred = tf.reshape(y_pred, [-1])
        
        # Clip the predicted values to avoid log(0)
        y_pred = tf.clip_by_value(y_pred, 1e-7, 1 - 1e-7)
        
        # Cast y_true to float32
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
        # Flatten the tensors
        y_true = tf.reshape(y_true, [-1])
        y_pred = tf.reshape(y_pred, [-1])
        
        # Clip the predicted values to avoid log(0)
        y_pred = tf.clip_by_value(y_pred, 1e-7, 1 - 1e-7)
        
        # Cast y_true to float32
        y_true = tf.cast(y_true, tf.float32)
        
        # Calculate the binary cross-entropy loss
        bce_loss = y_true * tf.math.log(y_pred) + (1 - y_true) * tf.math.log(1 - y_pred)
        
        # Apply the class weights to the loss
        class_weights_tensor = tf.gather(class_weights, tf.cast(y_true, tf.int32))
        weighted_bce_loss = class_weights_tensor * bce_loss
        
        # Return the mean weighted loss
        return -tf.reduce_mean(weighted_bce_loss)
    
    return loss

# Read in the CSV file 
df = pd.read_csv('MoveCNNAETrainingData.csv')

# Load the data
X = df.iloc[:,:-3].values
X = X.reshape(X.shape[0], 28, 28, 1)
Y = df.iloc[:,-3:].values

#Split into training / test
X_train, X_test, y_train, y_test = train_test_split(X, Y, test_size=0.2)

#compute class weights based on the frequency of each class in the training data
class_weights = class_weight.compute_class_weight(None, classes=np.unique(X_train), y=X_train.flatten())

# Encoder
encoder = Sequential()
encoder.add(Conv2D(8, (3, 3), activation='relu', input_shape=(28, 28, 1), kernel_regularizer=keras.regularizers.l2(0.001), padding='same'))
encoder.add(layers.Dropout(0.2))
encoder.add(MaxPooling2D((2, 2), padding='same'))

encoder.add(Conv2D(8, (3, 3), activation='relu', kernel_regularizer=keras.regularizers.l2(0.001), padding='same')) 
encoder.add(layers.Dropout(0.2))
encoder.add(MaxPooling2D((2, 2), padding='same'))

# Decoder
decoder = Sequential()
decoder.add(Conv2D(8, (3, 3), activation='relu', kernel_regularizer=keras.regularizers.l2(0.001), padding='same'))
decoder.add(layers.Dropout(0.2))
decoder.add(UpSampling2D((2, 2)))  

decoder.add(Conv2D(8, (3, 3), activation='relu', kernel_regularizer=keras.regularizers.l2(0.001), padding='same'))
decoder.add(layers.Dropout(0.2))
decoder.add(UpSampling2D((2, 2)))
decoder.add(Conv2D(1, (3, 3), activation='sigmoid', padding='same'))

# Autoencoder model
input = encoder.input
output = decoder(encoder.output)
autoencoder = Model(input, output)

zero_weight = 0.5  # Example weight for zero predictions
one_weight = 0.5   # Example weight for one predictions

# Compile
#autoencoder.compile(optimizer='adam', loss=binary_crossentropy, metrics=['accuracy'])#produces imbalanced class predictions biased to the majority class
autoencoder.compile(optimizer='adam', loss=weighted_binary_crossentropy(class_weights), metrics=['accuracy'])#same issue if class_weights use 'balanced' but not with None
#autoencoder.compile(optimizer='adam', loss=custom_weighted_binary_crossentropy(zero_weight, one_weight), metrics=['accuracy'])#isn't affected by the issue. 50/50 weighting

# Train
autoencoder.fit(X_train, X_train, epochs=512, batch_size=64)

#Model info
print("_______________________________________________________________________")
print("Model Info")
print("_______________________________________________________________________")
print("X shape:", X.shape)
print("Y shape:", Y.shape)
autoencoder.summary()
print("_______________________________________________________________________")
print("Encoder Info")
encoder.summary()
print("_______________________________________________________________________")
print("Decoder Info")
decoder.summary()


# Reconstruct the training images
reconstructed_train = autoencoder.predict(X_train)

# Apply a threshold to the reconstructed training images
threshold = 0.5
reconstructed_train_thresholded = np.where(reconstructed_train >= threshold, 1, 0)

# Compare the thresholded reconstructed training images with the original training images
train_accuracy = np.mean(np.equal(reconstructed_train_thresholded, X_train))
print("Reconstruction accuracy on training data:", train_accuracy)

# Reconstruct the test images
reconstructed_test = autoencoder.predict(X_test)

# Apply the same threshold to the reconstructed test images
reconstructed_test_thresholded = np.where(reconstructed_test >= threshold, 1, 0)

# Compare the thresholded reconstructed test images with the original test images
test_accuracy = np.mean(np.equal(reconstructed_test_thresholded, X_test))
print("Reconstruction accuracy on test data:", test_accuracy)


# Export the autoencoder
tf.saved_model.save(autoencoder, "saved_cnnae_model_dir")

# Export the encoder
tf.saved_model.save(encoder, "saved_cnne_model_dir")