using System;
using System.Linq;
using Random = UnityEngine.Random;

namespace Assets.Scripts {
    public class NeuralNetwork {
        private const double EpsilonInit = 2; //0.12; // Seems like a high epsilon makes the output more drastic in comparison with other outputs..
        private readonly int _hiddenLayerSize;
        private readonly double[] _hiddenNodes;
        private readonly double[] _hiddenWeights;
        private readonly int _inputLayerSize;

        private readonly double[] _inputNodes;

        private readonly double[] _inputWeights;
        private readonly int _outputLayerSize;
        private readonly double[] _outputNodes;

        public NeuralNetwork(int inputLayerSize, int hiddenLayerSize, int outputLayerSize, double[] inputWeights = null) {
            _inputLayerSize = inputLayerSize;
            _hiddenLayerSize = hiddenLayerSize;
            _outputLayerSize = outputLayerSize;

            // Nodes
            _inputNodes = new double[_inputLayerSize + 1]; // +1 for bias
            _hiddenNodes = new double[_hiddenLayerSize + 1]; // +1 for bias
            _outputNodes = new double[_outputLayerSize];

            // Add bias nodes
            const int bias = 1; // TODO: What is a good value here?
            _inputNodes[0] = bias;
            _hiddenNodes[0] = bias;

            // Weights
            if (inputWeights == null) {
                _inputWeights = new double[(_inputLayerSize + 1) * _hiddenLayerSize]; // +1 for bias
                for (int i = 0; i < _inputWeights.Length; i++) // Initialize weights randomly
                    _inputWeights[i] = Random.value * 2 * EpsilonInit - EpsilonInit;
            } else {
                _inputWeights = inputWeights;
            }

            _hiddenWeights = new double[(_hiddenLayerSize + 1) * _outputLayerSize]; // +1 for bias
            for (int i = 0; i < _hiddenWeights.Length; i++) // Initialize weights randomly
                _hiddenWeights[i] = Random.value * 2 * EpsilonInit - EpsilonInit;
        }

        public double[] FeedForward(double[] inputs) {
            if (inputs.Length != _inputLayerSize)
                throw new Exception($"input size ({inputs.Length}) does not match required size ({_inputLayerSize})");

            // Normalize input
            double[] normalizedInput = MinMaxNormalization(inputs); // TODO: Maybe disable this to get bigger variance in outputs
            for (int i = 1; i < _inputNodes.Length; i++) // Start at 1 to skip bias node
                _inputNodes[i] = normalizedInput[i - 1];

            // Activate hidden layer
            for (int hiddenLayerIndex = 1; hiddenLayerIndex < _hiddenNodes.Length; hiddenLayerIndex++) { // Start at 1 to skip hidden layer's bias node
                double hiddenNodeValue = 0;
                for (int inputLayerIndex = 0; inputLayerIndex < _inputNodes.Length; inputLayerIndex++) {
                    int offset = _inputNodes.Length * (hiddenLayerIndex - 1); // Weight index offset (-1 because the hidden layer loop starts at 1)
                    hiddenNodeValue += _inputNodes[inputLayerIndex] * _inputWeights[offset + inputLayerIndex];
                }

                _hiddenNodes[hiddenLayerIndex] = Sigmoid(hiddenNodeValue);
            }

            // Activate output layer
            for (int outputLayerIndex = 0; outputLayerIndex < _outputNodes.Length; outputLayerIndex++) { // Don't need to start at 1, because output layer doesn't have a bias node
                double outputNodeValue = 0;
                for (int hiddenLayerIndex = 0; hiddenLayerIndex < _hiddenNodes.Length; hiddenLayerIndex++) {
                    int offset = _hiddenNodes.Length * outputLayerIndex; // Weight index offset (no -1 necessary here)
                    outputNodeValue += _hiddenNodes[hiddenLayerIndex] * _hiddenWeights[offset + hiddenLayerIndex];
                }

                _outputNodes[outputLayerIndex] = Sigmoid(outputNodeValue);
            }

            return _outputNodes;
        }

        private double Sigmoid(double x) {
            return 1.0 / (Math.Exp(-x) + 1.0);
        }

        private double[] MinMaxNormalization(double[] input) {
            double[] normalizedInput = new double[input.Length];

            for (int i = 0; i < input.Length; i++) {
                double normalizedValue = (input[i] - input.Min()) / (input.Max() - input.Min());
                normalizedValue = double.IsNaN(normalizedValue) ? 0 : normalizedValue; // Convert NaN into 0

                normalizedInput[i] = normalizedValue;
            }

            return normalizedInput;
        }

        public int GetInputLayerSize() {
            return _inputLayerSize;
        }

        public int GetHiddenLayerSize() {
            return _hiddenLayerSize;
        }

        public int GetOutputLayerSize() {
            return _outputLayerSize;
        }

        public double[] GetInputWeights() {
            return _inputWeights;
        }
    }
}