import pandas as pd
from tensorflow import keras
from keras.models import Sequential
from keras.layers import Dense
from sklearn.metrics import classification_report
import numpy as np
from sklearn.model_selection import train_test_split
import tensorflow as tf

# Read in the CSV file 
df = pd.read_csv('data2.csv')

X = df.iloc[:,:-3].values
Y = df.iloc[:,-3:].values 

#Split into training / test
X_train, X_test, y_train, y_test = train_test_split(X, Y, test_size=0.2)

# Create the MLP model
model = keras.Sequential()
model.add(Dense(16, activation='relu', input_shape=(X.shape[1],)))
model.add(Dense(16))
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
tf.saved_model.save(model, "saved_model_dir_2")