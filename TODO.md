[x] 1. Save all training data types when saving recording. Filename: ExerciseName+NetworkName+.csv
[x] 2. Get example exercises recorded, save their output, and train

[x] 1. DataRepository - ExerciseName->LSTM,CNN,MLP,Start/End
[x] 2. UI that displays these exercises: Jumping jacks, situps, pushups
[x] 3. When an exericse is selected, publish ExerciseSelected Event & 
[x]    3.5 use DataRepository to initialize inference controllers
[x] 4. inference controllers are run each frame and their output is exposed
[x] 5. Prediction manager reads output from inference controllers each frame and weights their predictions to get final predictions
    [x] a. Weights need to be editable with a single slider. More data, or more user

[UI]
[x] 1. Make an exercise select view
[x] 2. exercise count view
[x] 3. exercise record view
[x] 4. Need some kind of intermediary in the settings. Maybe just a settings view
[x]  5. One button to "personalize" and one slider to change its value but it will have an error if theres no user data so it can't be edited until after

2.29 : 7 hours
2.1 : 8 hours
3.2 : 6 hours
3.3 : 2 hours


-> 4 hours
-> Adjust dashboard - SDK
-> how many events were sent?
-> What events do they want? 
   -> when a rewarded video is called?
-> look at documentation


12.10.24
10:00pm-11:00pm 1hr preparing video data
Total: 1hr

12.13.24
1:50pm-3:00pm 1hr 10m started video splicing
4:00pm-6:21pm 2hr 21m finishing video splicing
9:20pm-9:50pm 30m combining spliced videos into large batches
9:50pm-10:15pm 25m Double checking annotation tool before I use it
10:30pm-10:45pm 15m Adding a couple of tests based on some issues I noticed
11:30pm-12:24am 54m annotating a sample of video data
12:30am-1:30am 1hr training, testing, & re-annotating
Total: 6hr 35m

12.14.24
8:00pm-9:00pm 1hr Bug fixes ie occlusion & confidence score issues
Total: 1hr

12.15.24
2:30pm-3:00pm 30m Added a way to step through videos during annotation so it's not so fast
3:00pm-4:00pm 1hr Made exercises more configurable so previous ones weren't broken by updates
4:15pm-4:45pm 30m Bug fixing ie non-existent index issue
5:00pm-6:05pm 1hr 5m re-annotating, training & testing after bug fixes
1:10am-2:30am 1hr 20m annotating, training & testing 2nd batch & 1-2 combined (performance degraded... bad training examples)
Total: 4hr 25m

12.16.24
12:20pm-1:10pm 50m annotating, training & testing 3rd batch & 1-3 combined (performance degrades unless all data is highest quality) PushUps-test5-1-30
2:45pm-4:00pm 1hr 15m '' '' 2nd batch & 1-2 combined (performance IMPROVED) PushUps-test7-1-22
4:15pm-4:30pm 15m preparing training scene to record false positives
4:45pm-5:25pm 40m annotating, training & testing false positives in batch #1 (10% fp examples reduces issues from 50x to 2x) PushUps-test8-1-10-fp
5:30pm-5:45pm 15m training & testing w/ false positives in batch 1-2 combined (5% fp examples reduces issues from 50x to 5x) PushUps-test9-1-22-fp
7:00pm-7:45pm 45m '' '' except only for false 0 class (5% fp examples reduces issues from 50x to 1x) PushUps-test11-1-22-fp
8:30pm-9:35pm 1hr 5m '' '' except only for false 1 class (2% fp examples reduces issues from 80x to 8x but also made additional false negatives) PushUps-test12-1-22-fp
9:45pm-10:30pm 45m testing different versions in real time
10:45pm-11:15pm 30m finished testing
Total:  6hr 

12.17.24
[x] get chatgpt style agreement
1. re-annotate batch #3
2. train batch #3
3. test & repeat until requirements are met 
4. integrate into batch #1 & #2 dataset 
5. retrain 
6. collect false postives for 0 class, retrain, & repeat
7. update

1. take more videos w/ no exercises at all
2. fp save files w/ 0 class labels
3. retrain & test

1. Annotate video data again
   -> 1
   -> 2
   -> 3

2. Training scene -> needs to be live training
3. Alternative Training Scene -> needs to be video training

Data
[x] 1. Set Start/End position in ExerciseDataRepository by getting centroid on mlp training data. WILL need exercise name for this. Will need to setup train view first
[x] 2. Create UserExerciseData.cs
[x] 3. Attempt to load user preferences for start/end positions; but if they aren't available, use data from #1
[x] 4. Need a central location for UserExerciseData loading/saving


https://developer.apple.com/documentation/createml/creating-an-action-classifier-model

[] Test CNN output for pushups
-> observe the change in class output values during inference time
   -> Values change in a smooth-ish looking pattern
-> determine if they are as expected or if/what anomolies are 
   -> 1 class goes from 0% probability to 99% as 0 class goes from 99% to 0% when observation goes from 0 class to 1 class ie seems to be exactly as expected [lets call this observation a]
   -> however, upon further inspection after logging to console, it would appear that random spikes occur ie which create false positives & double counting [lets call this observation b]
   -> in addition to this, sometimes false negatives occur too, but very rarely [lets call this observation c]
-> A/B test output filtering algorithm & test accuacy, precision, etc 
   -> tested raw outputs vs average filtering vs hysteresis filter vs rate limiting, vs changes in heuristics, etc
      -> changed filtering algorithm to a more intuitive previous version during testing and found it to work better
   -> also found a change in thresholds to be pretty helpful in decreasing false positives
   -> the heuristics of the filtering algorithm also seemed to not directly consider "observation a" ie the pattern matching wasn't directly matching for the expected behavior in observation a
      -> upon updating this and the thresholds, went from 75% accuracy to 95% accuracy on 100 test examples
      -> also seemed to do well during live testing but no benchmarks were done due to being tired from creating training data
*************************************************************************************************************************
   It would seem the 1 label is easily differentiable
   However, 0 label fluctuates from what looks like a 0 label and then as the user gets closer to the ground, (unconfident) predictions shift it to a 1. 
   Then naturally as they come back to a more 0 label, it confidently predicts 0 
   then the user successfully goes to a 1 
   only a single one of the transitions should have been counted
   "If an action classifier misidentifies a nonaction as an action, create or augment a negative class with examples of that irrelevant action"
*************************************************************************************************************************

[] Increase training data
-> record video data 
-> splice it using obs so it only contains data
-> get video data labeling working in scene again
-> label the data
   -> Scores?
   -> Class Probabilities?
   -> in either scenario "observation a" could occur again so adaquate dataset size should be tested first
-> apply simple class oversampling upon save
-> retrain CNN and observe accuracy, precision, etc
-> then retest against video data in scene & observe accuracy, precision, etc
-> then retest against live 
*************************
->Augmentation
   you can effectively double your training data by enabling the Horizontal Flip augmentation
*************************

Reconstruction accuracy on training data: 0.9607633703888059
Reconstruction accuracy on test data:     0.9603902371759515


_______________________________________________________________________
Train Info
_______________________________________________________________________
20/20 [==============================] - 1s 2ms/step
              precision    recall  f1-score   support

           0       0.96      0.97      0.97       109
           1       0.98      1.00      0.99       115
           2       0.99      0.99      0.99       414

    accuracy                           0.99       638
   macro avg       0.98      0.99      0.98       638
weighted avg       0.99      0.99      0.99       638

_______________________________________________________________________
Test Info
_______________________________________________________________________
5/5 [==============================] - 0s 2ms/step
              precision    recall  f1-score   support

           0       0.91      0.79      0.85        38
           1       0.76      0.91      0.83        35
           2       0.87      0.85      0.86        87

    accuracy                           0.85       160
   macro avg       0.85      0.85      0.85       160
weighted avg       0.86      0.85      0.85       160

try feeding both vectors in