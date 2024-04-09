import pandas as pd
import numpy as np
from sklearn.model_selection import train_test_split
from keras.models import Sequential
from keras.layers import LSTM, Dense, Flatten
from sklearn.metrics import classification_report
from tensorflow import keras
from keras import layers
import tensorflow as tf
from sklearn.utils import class_weight

def weighted_categorical_crossentropy(class_weights):
    class_weights = tf.cast(class_weights, tf.float32)
    
    def loss(y_true, y_pred):
        # Clip the predicted values to avoid log(0)
        y_pred = tf.clip_by_value(y_pred, 1e-7, 1 - 1e-7)
        
        # Calculate the categorical cross-entropy loss
        cce_loss = tf.keras.losses.categorical_crossentropy(y_true, y_pred)
        
        # Apply the class weights to the loss
        weighted_cce_loss = tf.multiply(cce_loss, tf.reduce_sum(tf.multiply(class_weights, y_true), axis=-1))
        
        # Return the mean weighted loss
        return tf.reduce_mean(weighted_cce_loss)
    
    return loss

# Read in the CSV file
df = pd.read_csv('MoveLSTMTrainingData.csv')

# Sliding window function
def sliding_windows(data, seq_length, target_pos):
    X,Y = [], []
    for i in range(len(data)-seq_length):
        X.append(data.iloc[i : i+seq_length, :target_pos])
        Y.append(data.iloc[i+seq_length, target_pos:])
    return np.array(X), np.array(Y)

# Prepare training data
seq_length = 8
target_pos = -3 # Third last columns as target
X, Y = sliding_windows(df, seq_length, target_pos)

#Split into training / test
X_train, X_test, y_train, y_test = train_test_split(X, Y, test_size=0.3)

# Compute class weights based on the frequency of each class in the training data
#class_weights = class_weight.compute_class_weight('balanced', classes=np.unique(np.argmax(y_train, axis=1)), y=np.argmax(y_train, axis=1))
class_weights = {0: 0.333, 1: 0.333, 2: 0.333}
value_array = np.array(list(class_weights.values()))


# Build LSTM
model = Sequential()
model.add(LSTM(32, kernel_regularizer=keras.regularizers.l2(0.001), input_shape=(seq_length, X.shape[-1]), return_sequences=True))
model.add(layers.Dropout(0.2))
model.add(LSTM(32, kernel_regularizer=keras.regularizers.l2(0.001)))
model.add(layers.Dropout(0.2))
model.add(Dense(32, activation='relu', kernel_regularizer=keras.regularizers.l2(0.001)))
model.add(layers.Dropout(0.2))
model.add(Dense(Y.shape[1], activation='softmax'))

# Compile and train
model.compile(optimizer='adam', loss=weighted_categorical_crossentropy(value_array), metrics=['accuracy'])
#model.compile(loss='categorical_crossentropy', optimizer='adam', metrics=['acc'])
model.fit(X_train, y_train, epochs=256, batch_size=32)

#Model info
print("_______________________________________________________________________")
print("Model Info")
print("_______________________________________________________________________")
print("X shape:", X.shape)
print("Y shape:", Y.shape)
model.summary()

#Classification report info - Train
print("_______________________________________________________________________")
print("Train Info")
print("_______________________________________________________________________")
predictions_train = model.predict(X_train)
pred_classes_train = np.argmax(predictions_train, axis=1)
true_classes_train = np.argmax(y_train, axis=1)
print(classification_report(true_classes_train, pred_classes_train))

#Classification report info - Test
print("_______________________________________________________________________")
print("Test Info")
print("_______________________________________________________________________")
predictions_test = model.predict(X_test)
pred_classes_test = np.argmax(predictions_test, axis=1)
true_classes_test = np.argmax(y_test, axis=1)
print(classification_report(true_classes_test, pred_classes_test))

# Export the model
tf.saved_model.save(model, "saved_lstm_model_dir")