# Machine Learning Exercise Classification

### Demo Video
https://www.youtube.com/shorts/YLEuQXkwqGU  

## Procedure

### Body Pose Prediction Model 
I used MoveNet Single Pose Lightning (tflite - quantized to float16)

https://www.kaggle.com/models/google/movenet/tfLite/singlepose-lightning-tflite-float16

### Step 1 - Training Data Collection
Record footage of someone doing an exercise in the desired screen orientation. ie Pushups in Portrait. Aim to get an even distributuion of examples whereby the subject in the video is recorded with a horizontal and vertical offset. Simply put, ensure the person in the video is to the left/right and close/further from the camera in each shot.

### Step 2 - Data Pre-Processing - Transformation (success was observed with and without this step)
The dense vector output from the body pose prediction model gets some transformations applied to it. Firstly, an "anchor" point is decided upon. The head was chosen in this example. After which, the original offsets between the head and all other points are computed. New points are projected from the anchor points via the offsets. (This step is done to create position invariance) However, uniform magnitudes are chosen. (This step is done to yield scale invariance) The uniform magnitudes yield a final transformation which looks like a "boxy" person with equally distant arms, legs, etc At this point, regardless of if a person being recorded moves left/right they appear in a somewhat similar position horizontally & vertically speaking. And regardless of if they back up or get closer, they appear as a similar size.

### Step 3 - Data Pre-Processing - Filtering
In order to prevent vectors in the body pose prediction model from interfering with each other, a filtering process is used. Essentially, the body pose prediction model outputs a dense vector of points. However, more focus was placed on the vectors computed between the points ie the left forearm, left upperarm, etc. Those vectors or lines were extracted by simply creating pairs of points that were used to generate the lines. Similar to what is shown the users. Except the fact that per exercise, "important" lines are chosen and "unimportant" lines are completely filtered out. (This step is done to prevent spurlious correlations) This prevents overlap of lines in the next step. However it was noted that simply adding additional channels would have proven to suffice but that option wasn't explored yet. 

### Step 4 - Data Pre-Processing - Reprojection
After these data pre-processing steps are completed, one final step of data pre-processing is undergone. The final set of lines are projected onto a 28x28 grid to create a one-hot encoded array to act as a 2D image. This image looks like what the user typically sees except for looking like a "boxy" person as previously mentioned.

### Step 5 - Annotation
The video footage from step #1 is loaded into a piece of custom software for video annotation. The video gets monitored frame by frame, with step #1-#4 applied as a developer simply uses an xbox controller to label up/down positions. The indicies of the frames of these labeled positions are noted such that the previous -1 and future +1 indicies can also be labeled with the same labels. (ie class oversampling augmentation) This effectively triples training data. However to take it one step further, the same exact videos are processed again after being horizonally flipped (ie horizontal flip augmentation) This effectively doubles the training data again afterwards. So each "training sample" ie 1 complete exercise becomes 6 up examples and 6 down examples. The data gets saved as comma seperated values of input/output pairs.

### Step 6 - Training
Naturally this csv data, merely being a representation of a 2D image, is fed into a Convultional Neural Network (CNN) during training time and multiclass classifcation is used to learn class probabilites. Depending on when it was done, at one point only 2 classes were used, however it was noted to be impractical due to false positives. So 1-2 additional channels were tested. Whereby each additional channel was simply a false positive channel for the up/down predictions. The final model was saved in onnx format. A 1.0 f1 score was observed.

### Step 7 - False Positive Annotation
Additional video footage was collected without any up/down examples. This video was passed back to the video annotation software and false positives were automatically labeled. This is due to the fact that any detected classification was known to be incorrect. So it was simply auto-labeled as a false positive for the corresponding channel. The false positive data was then introduced into the orignal training dataset and then the model was retrained.

### Step 8 - Inference
Finally, the classification model, was used at inference time with a few filters to yield accurate counting. A rate limiter filter was applied. Same goes with a hysteresis filter. High class probability thresholds were utilized ie 95% confidence at count time. It was noted that a PID controller could have been utilized at this point too but didn't seem to be necessary.
