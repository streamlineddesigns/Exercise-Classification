import pandas as pd
import tensorflow as tf
from tensorflow import keras
from tensorflow.keras.models import Sequential
from keras import layers
from tensorflow.keras.layers import Flatten, Conv2D, MaxPooling2D, Dense
from tensorflow.keras.optimizers import Adam
import numpy as np
from sklearn.metrics import classification_report
from sklearn.model_selection import train_test_split

# Read in the CSV file 
df = pd.read_csv('MoveCNNTrainingData.csv')

# Load the data
X = df.iloc[:,:-3].values
X = X.reshape(X.shape[0], 28, 28, 1)
Y = df.iloc[:,-3:].values

#Split into training / test
X_train, X_test, y_train, y_test = train_test_split(X, Y, test_size=0.2)

# Define the model
model = Sequential()
model.add(Conv2D(32, (3, 3), activation='relu', input_shape=(28, 28, 1), kernel_regularizer=keras.regularizers.l2(0.001)))
model.add(layers.Dropout(0.2))
model.add(MaxPooling2D((2, 2)))
model.add(Flatten())
model.add(Dense(64, activation='relu', kernel_regularizer=keras.regularizers.l2(0.001)))
model.add(layers.Dropout(0.2))
model.add(Dense(32, activation='relu', kernel_regularizer=keras.regularizers.l2(0.001)))
model.add(layers.Dropout(0.2))
model.add(Dense(Y.shape[1], activation='softmax'))
# Compile the model
model.compile(loss='categorical_crossentropy', optimizer='adam', metrics=['accuracy'])
# Train the model
model.fit(X_train, y_train, epochs=128, batch_size=32)

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
tf.saved_model.save(model, "saved_cnn_model_dir")