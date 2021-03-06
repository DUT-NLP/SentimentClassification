#-*-coding:utf-8-*-
import os
import sys
import timeit
import cPickle
import numpy

import theano
import theano.tensor as T
from theano.tensor.signal import downsample
from theano.tensor.nnet import conv

from logistic_sgd import LogisticRegression, load_data
from Mlp import HiddenLayer


from wc import wc
from writeNonsenceLabel import writeNonsenceLabel
from writeNonsenceVector import writeNonsenceVector

class LeNetConvPoolLayer(object):
    """Pool Layer of a convolutional network """

    def __init__(self, rng, input, filter_shape, image_shape, poolsize=(2, 2)):
        """
        Allocate a LeNetConvPoolLayer with shared variable internal parameters.

        :type rng: numpy.random.RandomState
        :param rng: a random number generator used to initialize weights

        :type input: theano.tensor.dtensor4
        :param input: symbolic image tensor, of shape image_shape

        :type filter_shape: tuple or list of length 4
        :param filter_shape: (number of filters, num input feature maps,
                              filter height, filter width)

        :type image_shape: tuple or list of length 4
        :param image_shape: (batch size, num input feature maps,
                             image height, image width)

        :type poolsize: tuple or list of length 2
        :param poolsize: the downsampling (pooling) factor (#rows, #cols)

        """

        assert image_shape[1] == filter_shape[1]
        self.input = input

        # there are "num input feature maps * filter height * filter width"
        # inputs to each hidden unit
        fan_in = numpy.prod(filter_shape[1:])
        # each unit in the lower layer receives a gradient from:
        # "num output feature maps * filter height * filter width" /
        #   pooling size
        fan_out = (filter_shape[0] * numpy.prod(filter_shape[2:]) /
                   numpy.prod(poolsize))
        # initialize weights with random weights
        W_bound = numpy.sqrt(6. / (fan_in + fan_out))
        self.W = theano.shared(
            numpy.asarray(
                rng.uniform(low=-W_bound, high=W_bound, size=filter_shape),
                dtype=theano.config.floatX
            ),
            borrow=True
        )

        # the bias is a 1D tensor -- one bias per output feature map
        b_values = numpy.zeros((filter_shape[0],), dtype=theano.config.floatX)
        self.b = theano.shared(value=b_values, borrow=True)

        # convolve input feature maps with filters
        conv_out = conv.conv2d(
            input=input,
            filters=self.W,
            filter_shape=filter_shape,
            image_shape=image_shape
        )

        # downsample each feature map individually, using maxpooling
        pooled_out = downsample.max_pool_2d(
            input=conv_out,
            ds=poolsize,
            ignore_border=True
        )

        # add the bias term. Since the bias is a vector (1D array), we first
        # reshape it to a tensor of shape (1, n_filters, 1, 1). Each bias will
        # thus be broadcasted across mini-batches and feature map
        # width & height
        self.output = T.tanh(pooled_out + self.b.dimshuffle('x', 0, 'x', 'x'))

        # store parameters of this layer

        self.params = [self.W, self.b]

        # keep track of model input
        self.input = input
def ReLU(x):
    y = T.maximum(0.0, x)
    return(y)
def evaluate_lenet5(learning_rate=0.1, n_epochs=10,
                    dataset = "dataSet.pkl", clas = "book", dimension=50, filter=3, map=200, batch_size=200, maxLen=102, poolSize=2):
    """ Demonstrates lenet on MNIST dataset

    :type learning_rate: float
    :param learning_rate: learning rate used (factor for the stochastic
                          gradient)

    :type n_epochs: int
    :param n_epochs: maximal number of epochs to run the optimizer

    :type dataset: string
    :param dataset: path to the dataset used for training /testing (MNIST here)

    :type nkerns: list of ints
    :param nkerns: number of kernels on each layer
    """

    model_options = locals().copy()
    print "model options", model_options

    nkerns=[map]

    rng = numpy.random.RandomState(23455)

    datasets = load_data(dataset)

    train_set_x, train_set_y = datasets[0]
    test_set_x, test_set_y = datasets[1]

    # compute number of minibatches for training, validation and testing
    n_train_batches = train_set_x.get_value(borrow=True).shape[0]
    print(train_set_x.get_value(borrow=True).shape[0])
    n_test_batches = test_set_x.get_value(borrow=True).shape[0]
    print(test_set_x.get_value(borrow=True).shape[0])
    n_train_batches /= batch_size
    n_test_batches /= batch_size


    # allocate symbolic variables for the data
    index = T.lscalar()  # index to a [mini]batch

    # start-snippet-1
    x = T.matrix('x')   # the data is presented as rasterized images
    y = T.ivector('y')  # the labels are presented as 1D vector of
                        # [int] labels

    ######################
    # BUILD ACTUAL MODEL #
    ######################
    print '... building the model'

    # Reshape matrix of rasterized images of shape (batch_size, 28 * 28)
    # to a 4D tensor, compatible with our LeNetConvPoolLayer
    # (28, 28) is the size of MNIST images.
    #layer0_input = x.reshape((batch_size, 1, dimension, maxLen))

    # Construct the first convolutional pooling layer:
    # filtering reduces the image size to (28-5+1 , 28-5+1) = (24, 24)
    # maxpooling reduces this further to (24/2, 24/2) = (12, 12)
    # 4D output tensor is thus of shape (batch_size, nkerns[0], 12, 12)
    # layer0 = LeNetConvPoolLayer(
    #     rng,
    #     input=layer0_input,
    #     image_shape=(batch_size, 1, dimension, maxLen),
    #     filter_shape=(nkerns[0], 1, dimension, filter),
    #     poolsize=(1, poolSize)
    # )

    # Construct the second convolutional pooling layer
    # filtering reduces the image size to (12-5+1, 12-5+1) = (8, 8)
    # maxpooling reduces this further to (8/2, 8/2) = (4, 4)
    # 4D output tensor is thus of shape (batch_size, nkerns[1], 4, 4)
    # layer1 = LeNetConvPoolLayer(
    #     rng,
    #     input=layer0.output,
    #     image_shape=(batch_size, nkerns[0], 12, 12),
    #     filter_shape=(nkerns[1], nkerns[0], 5, 5),
    #     poolsize=(2, 2)
    # )

    # the HiddenLayer being fully-connected, it operates on 2D matrices of
    # shape (batch_size, num_pixels) (i.e matrix of rasterized images).
    # This will generate a matrix of shape (batch_size, nkerns[1] * 4 * 4),
    # or (500, 50 * 4 * 4) = (500, 800) with the default values.
    layer1_input = x.reshape((batch_size, 1, dimension, 1)).flatten(2)

    # construct a fully-connected sigmoidal layer
    layer1 = HiddenLayer(
        rng,
        input=layer1_input,
        n_in=dimension,
        n_out=100,
        activation=ReLU
    )

    layer2_input = layer1.output

    layer2 = HiddenLayer(
        rng,
        input=layer2_input,
        n_in=100,
        n_out=50,
        activation=ReLU
    )

    # classify the values of the fully-connected sigmoidal layer
    layer3 = LogisticRegression(input=layer2.output, n_in=50, n_out=2)

    # the cost we minimize during training is the NLL of the model
    cost = layer3.negative_log_likelihood(y)

    # create a function to compute the mistakes that are made by the model
    test_model = theano.function(
        [index],
        layer3.errors(y),
        givens={
            x: test_set_x[index * batch_size: (index + 1) * batch_size],
            y: test_set_y[index * batch_size: (index + 1) * batch_size]
        }
    )

    test_pred = theano.function(
        [index],
        layer3.get_p_y_given_x(),
        givens={
            x: test_set_x[index * batch_size: (index + 1) * batch_size]
        }
    )
    test_layer2_out = theano.function(
        [index],
        layer2.output,
        givens={
            x: test_set_x[index*batch_size: (index+1) * batch_size]}
    )
    test_layer1_out = theano.function(
        [index],
        layer1.output,
        givens={
            x: test_set_x[index*batch_size: (index+1) * batch_size]}
    )
    # create a list of all model parameters to be fit by gradient descent
    # params = layer3.params + layer2.params + layer1.params + layer0.params
    params = layer3.params + layer2.params + layer1.params# + layer0.params

    # create a list of gradients for all model parameters
    grads = T.grad(cost, params)

    # train_model is a function that updates the model parameters by
    # SGD Since this model has many parameters, it would be tedious to
    # manually create an update rule for each model parameter. We thus
    # create the updates list by automatically looping over all
    # (params[i], grads[i]) pairs.
    updates = [
        (param_i, param_i - learning_rate * grad_i)
        for param_i, grad_i in zip(params, grads)
    ]

    train_model = theano.function(
        [index],
        cost,
        updates=updates,
        givens={
            x: train_set_x[index * batch_size: (index + 1) * batch_size],
            y: train_set_y[index * batch_size: (index + 1) * batch_size]
        }
    )
    # end-snippet-1

    ###############
    # TRAIN MODEL #
    ###############
    print '... training'
    # early-stopping parameters
    patience = 10000  # look as this many examples regardless
    patience_increase = 2  # wait this much longer when a new best is
                           # found
    improvement_threshold = 0.995  # a relative improvement of this much is
                                   # considered significant
    validation_frequency = min(n_train_batches, patience / 2)
                                  # go through this many
                                  # minibatche before checking the network
                                  # on the validation set; in this case we
                                  # check every epoch
    # validation_frequency = n_train_batches/5

    best_validation_loss = numpy.inf
    best_iter = 0
    test_score = 0.
    start_time = timeit.default_timer()

    epoch = 0

    while (epoch < n_epochs):
        epoch = epoch + 1
        for minibatch_index in xrange(n_train_batches):

            iter = (epoch - 1) * n_train_batches + minibatch_index

            if iter % 100 == 0:
                print 'training @ iter = ', iter
            cost_ij = train_model(minibatch_index)

            if (iter + 1) % validation_frequency == 0:

                # compute zero-one loss on validation set
                test_losses = [test_model(i) for i
                                     in xrange(n_test_batches)]
                this_validation_loss = numpy.mean(test_losses)
                print('epoch %i, minibatch %i/%i, cost %f test error %f %%' %
                      (epoch, minibatch_index + 1, n_train_batches, cost_ij,
                       this_validation_loss * 100.))

                # if we got the best validation score until now
                if this_validation_loss < best_validation_loss:

                    #improve patience if loss improvement is good enough
                    if this_validation_loss < best_validation_loss *  \
                       improvement_threshold:
                        patience = max(patience, iter * patience_increase)

                    # save best validation score and iteration number
                    best_validation_loss = this_validation_loss
                    best_iter = iter

                    # test it on the test set
                    test_losses = [
                        test_model(i)
                        for i in xrange(n_test_batches)
                    ]
                    test_score = numpy.mean(test_losses)
                    print(('     epoch %i, minibatch %i/%i, cost %f test error of '
                           'best model %f %%') %
                          (epoch, minibatch_index + 1, n_train_batches, cost_ij,
                           test_score * 100.))
        test_Matrix_out = numpy.asarray([test_layer2_out(i) for i in xrange(n_test_batches)])
        # print type(test_Matrix_out)
        # print test_Matrix_out.ndim
        print test_Matrix_out.shape
        numpy.savetxt(os.path.dirname(dataset) + "/"+clas+'_output_'+str(epoch)+'.txt', test_Matrix_out.reshape(n_test_batches * batch_size, 50), fmt='%.4f', delimiter=' ')
        result_Matrix = numpy.asarray([test_pred(i) for i in xrange(n_test_batches)])
        print result_Matrix.shape
        numpy.savetxt(os.path.dirname(dataset) + "/"+clas+'_test_pred_'+str(epoch)+'.txt', result_Matrix.reshape(n_test_batches * batch_size, 2), fmt='%.5f', delimiter=' ')
        # numpy.savetxt(embedName+'/'+str(dimension)+'/filter'+str(filter)+'_map'+str(map)+'_result_'+str(epoch)+'.txt',
        #               result_Matrix.reshape(n_test_batches * batch_size, 2), fmt='%.4f', delimiter=' ')
        # numpy.savetxt('_result_'+str(epoch)+'.txt',result_Matrix.reshape(n_test_batches * batch_size, 2), fmt='%.4f', delimiter=' ')
        # numpy.savetxt(embedName+'/'+str(dimension)+'/filter'+str(filter)+'_map'+str(map)+'_embedding'+str(epoch)+'.txt',
        #               layer0_input, fmt='%.4f', delimiter=' ')

    end_time = timeit.default_timer()
    print('Optimization complete.')
    print('Best validation score of %f %% obtained at iteration %i, '
          'with test performance %f %%' %
          (best_validation_loss * 100., best_iter + 1, test_score * 100.))
    print >> sys.stderr, ('The code for file ' +
                          os.path.split(__file__)[1] +
                          ' ran for %.2fm' % ((end_time - start_time) / 60.))


def preprocess(trainVectorFilePath, trainLabelFilePath, testVectorFilePath, testLabelFilePath, dataSetPath, wordDimension, batch_size = 200):

    print('Converting data format...')
    train_data_x = []
    train_data_y = []
    test_data_x = []
    test_data_y = []
    # train_vec_file = open("H:/CNN_YANG/"+embedName+"/train_"+str(wordDimension+posDimension)+".txt", 'r')
    # train_label_file = open("H:/CNN_YANG/"+embedName+"/train_label_"+str(wordDimension+posDimension)+".txt", 'r')
    
    #############################################
    AddVectorCountToTimesOfBatch_size(trainVectorFilePath, batch_size, wordDimension)
    AddLabelCountToTimesOfBatch_size(trainLabelFilePath, batch_size)
    #############################################
    train_vec_file=open(trainVectorFilePath, 'r')
    train_label_file = open(trainLabelFilePath, 'r')
    # train_vec_file=open("yuliao/book/outEmbedding_Trim.txt", 'r')
    # train_label_file = open("yuliao/book/dvd_label.txt", 'r')
    vectorCount = wc(trainVectorFilePath)
    
    for i in range(0, vectorCount):
        vec_line = train_vec_file.readline().strip()

        label_line = train_label_file.readline().strip()
        train_data_x.append([float(elem) for elem in vec_line.split(' ')])
        if label_line[0] == '1':
            train_data_y.extend([1])
        else:
            train_data_y.extend([0])

    test_vec_file = open(testVectorFilePath, 'r')
    test_label_file = open(testLabelFilePath, 'r')

    for i in range(0, vectorCount):
        vec_line = test_vec_file.readline().strip()
        label_line = test_label_file.readline().strip()
        test_data_x.append([float(elem) for elem in vec_line.split(' ')])
        if label_line[0] == '1':
            test_data_y.extend([1])
        else:
            test_data_y.extend([0])

    output_file = open(dataSetPath, 'wb')

    train_data = [train_data_x, train_data_y]
    test_data = [test_data_x, test_data_y]
    cPickle.dump(train_data, output_file)
    cPickle.dump(test_data, output_file)

    output_file.close()

def AddVectorCountToTimesOfBatch_size(fileName, batch_size, wordDimension):
    count = wc(fileName)
    countNeedToAdd = (batch_size - (count % batch_size))%batch_size
    writeNonsenceVector(fileName, countNeedToAdd, wordDimension)
    
    
def AddLabelCountToTimesOfBatch_size(fileName, batch_size):
    count = wc(fileName)
    countNeedToAdd = (batch_size - (count % batch_size))%batch_size
    writeNonsenceLabel(fileName, countNeedToAdd)

from multiprocessing import Process
import os
import time
from AddZerosVectorToSent import AddZerosVectorToSent
from genSentenceVector import genSentenceVector
import json

def SingleProcess(CorpusPath, CnnOutputPath, wordDimension, clas):

    posDimension = 0
    representationDim = 50    
    corpusPath = CorpusPath
    cnnOutputPath = CnnOutputPath

    branchPath = str(wordDimension)+"d/"+clas+"/"
    if(not os.path.exists(cnnOutputPath + branchPath)):
        os.makedirs(cnnOutputPath + branchPath)
                    
    datasetPath = cnnOutputPath + branchPath + clas+"_dataSet"+str(wordDimension)+".pkl"
    trainVectorFilePath = corpusPath + str(wordDimension) + "d/"+clas+"/"+clas+"_train_embed_EN.docm"
    trainLabelFilePath = "/home/laboratory/corpus/Serializer/train_music_label.txt"
    testVectorFilePath = corpusPath + str(wordDimension) + "d/"+clas+"/"+clas+"_test_embed_CN.docm"
    testLabelFilePath = "/home/laboratory/corpus/Serializer/test_music_label.txt"
    

    #if(not os.path.exists(datasetPath)):
    preprocess(trainVectorFilePath, trainLabelFilePath,testVectorFilePath,testLabelFilePath, dataSetPath = datasetPath, wordDimension = wordDimension, batch_size = 200)
    evaluate_lenet5(n_epochs=50, dataset = datasetPath, clas = clas, dimension=wordDimension+posDimension, filter=3, map=200, batch_size=200, maxLen=5, poolSize=1)
    # print(str(wordDimension) + " " + language + " " + clas + " is done. PID: " + str(os.getpid()))

    # numberFile = corpusPath+language+"/"+corpusType+"_"+clas+"_new.txt.number"
    # fragmentVectorFile = cnnOutputPath+str(wordDimension)+"d/"+language+"/"+clas+"/"+clas+"_output_40.txt"
    # indexFile = cnnOutputPath+str(wordDimension)+"d/"+language+"/"+clas+"/" + ""+corpusType+"_"+clas+"_new.txt.index"
    # sentenceVectorFile = cnnOutputPath+str(wordDimension)+"d/"+language+"/"+clas+"/" + ""+corpusType+"_"+clas+"_new.txt.sent"
    # genSentenceVector(numberFile, fragmentVectorFile, indexFile, sentenceVectorFile, representationDim)
    
    # branchPath = str(wordDimension)+"d/"+language+"/"+clas+"/"
    # indexFile = cnnOutputPath + branchPath + corpusType+"_"+clas+"_new.txt.index"
    # sentFile = cnnOutputPath + branchPath + corpusType+"_"+clas+"_new.txt.sent"
    # numberFile = corpusPath + language + "/"+corpusType+"_"+clas+"_new.txt.number"
    # newSentFile = cnnOutputPath + branchPath +clas+"_"+corpusType+"_embed_" +str.upper(language)+".sent"
    # newindexFile = cnnOutputPath + branchPath +clas+"_"+corpusType+"_index_" +str.upper(language)+".sent"
    # AddZerosVectorToSent(indexFile, sentFile, numberFile, newSentFile, newindexFile, representationDim)
                
if __name__ == '__main__':
  
    cnnJson = open("cnn.json", "r")
    inputInfo = json.load(cnnJson)
    cnnJson.close()

    corpusRootPath = "/home/laboratory/corpus/cnn_output/data/"
    cnnOutputRootPath = "/home/laboratory/corpus/cnn_output/classification"

    classes = ["music"]#, "book", "dvd"]
    wordDimensions = [50]#, 100]
    languages = ["EN", "CN"]
    processCount = 0
    for wordDimension in wordDimensions:
        for clas in classes:
            #SingleProcess(wordDimension, language, clas)
            p = Process(target=SingleProcess, args=(corpusRootPath, cnnOutputRootPath, wordDimension,clas))
            p.start()
            print(str(wordDimension) + " " + language + " " + clas + " is running. PID: " + str(p.ident))
                    
            processCount += 1
            #if(processCount % 6 == 0): p.join()
            p.join() # one by one.