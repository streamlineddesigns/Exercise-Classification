sudo apt update

wget "https://repo.anaconda.com/miniconda/Miniconda3-py37_4.12.0-Linux-x86_64.sh"
chmod 777 Miniconda3-py37_4.12.0-Linux-x86_64.sh
./Miniconda3-py37_4.12.0-Linux-x86_64.sh

conda create -n ml python=3.7
conda activate ml

pip install numpy
pip install pandas
pip install keras
pip install tensorflow
pip install scikit-learn
pip install tf2onnx

python LSTMTrainer.py
python -m tensorflow.python.tools.saved_model_cli show --dir "./saved_lstm_model_dir" --tag_set serve --signature_def serving_default
python -m tf2onnx.convert --saved-model "./saved_lstm_model_dir"  --opset 9 --inputs lstm_input:0 --inputs-as-nchw lstm_input:0 --output LSTM.onnx

python MLPTrainer.py
python -m tensorflow.python.tools.saved_model_cli show --dir "./saved_mlp_model_dir" --tag_set serve --signature_def serving_default
python -m tf2onnx.convert --saved-model "./saved_mlp_model_dir"  --opset 9 --inputs dense_input:0 --inputs-as-nchw dense_input:0 --output MLP.onnx

python CNNTrainer.py
python -m tensorflow.python.tools.saved_model_cli show --dir "./saved_cnn_model_dir" --tag_set serve --signature_def serving_default
python -m tf2onnx.convert --saved-model "./saved_cnn_model_dir"  --opset 9 --inputs conv2d_input:0 --inputs-as-nchw conv2d_input:0 --output CNN.onnx

python CNNAETrainer.py
python -m tensorflow.python.tools.saved_model_cli show --dir "./saved_cnnae_model_dir" --tag_set serve --signature_def serving_default
python -m tf2onnx.convert --saved-model "./saved_cnnae_model_dir"  --opset 11 --inputs conv2d_input:0 --inputs-as-nchw conv2d_input:0 --output CNNAE.onnx

python -m tensorflow.python.tools.saved_model_cli show --dir "./saved_cnne_model_dir" --tag_set serve --signature_def serving_default
python -m tf2onnx.convert --saved-model "./saved_cnne_model_dir"  --opset 11 --inputs conv2d_input:0 --inputs-as-nchw conv2d_input:0 --output CNNE.onnx

python VAETrainer.py
python -m tensorflow.python.tools.saved_model_cli show --dir "./saved_vae_model_dir" --tag_set serve --signature_def serving_default
python -m tf2onnx.convert --saved-model "./saved_vae_model_dir"  --opset 11 --inputs encoder_input:0 --inputs-as-nchw encoder_input:0 --output VAE.onnx

python -m tensorflow.python.tools.saved_model_cli show --dir "./saved_ve_model_dir" --tag_set serve --signature_def serving_default
python -m tf2onnx.convert --saved-model "./saved_ve_model_dir"  --opset 11 --inputs encoder_input:0 --inputs-as-nchw encoder_input:0 --output VE.onnx