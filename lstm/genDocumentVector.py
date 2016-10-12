import numpy
import argparse

def gendocumentVector(numberFile, fragmentVectorFile, indexFile, documentVectorFile, dimension):

    psnumber = open(numberFile,"r")
    fragmentVectors = open(fragmentVectorFile, "r")
    index = open(indexFile, "w")
    documentVectors = open(documentVectorFile, "w")
    
    graphNumber = 0
    
    #################################
    sentenceNumber = 0
    #################################
    
    curDocumentN = 1
    psnumber.readline()
    while True:
        sentenceCount = 1
        graphNumber +=1
        record = []
        for line in psnumber:
            record = line.strip().split(" ")
            if(int(record[0]) == curDocumentN):
                sentenceCount += 1
            else:
                curDocumentN = int(record[0])
                break
        if len(record) == 0: break
        documentVector = numpy.zeros(dimension)
        for i in range(sentenceCount):
            tmpSentenceVec = fragmentVectors.readline().rstrip().split(" ")
            sentenceNumber += 1
            documentVector += numpy.array([float(elem) for elem in tmpSentenceVec])
                
        documentVectorStr = ""
        for elem in documentVector:
            documentVectorStr += str(elem) + " "
        documentVectors.write(documentVectorStr + "\n")
        index.write(str(graphNumber) + "\n")
    print(str(sentenceNumber) + "\n" + indexFile + "\n" + documentVectorFile + " done.\n")        

import os
from multiprocessing import Process
if __name__ == "__main__":
#def genSen():
    mDimension = 50
    classes = ["music"]
    wordDimensions = [50]
    languages = ["EN", "CN"]
    dataTypes = ['train', 'test']
    #/home/laboratory/corpus/cnn_output/data/50d/music/music_test_index_EN.sent
    corpusPath = "/home/laboratory/corpus/"
    cnnOutputPath = "/home/laboratory/corpus/cnn_output/data/"
    for dataType in dataTypes:
        for clas in classes:
            for wordDimension in wordDimensions:
                for language in languages:
                    numberFile = cnnOutputPath+str(wordDimension)+"d/"+clas+"/"+clas+"_" + dataType + "_index_" + language + ".sent"
                    fragmentVectorFile = cnnOutputPath+str(wordDimension)+"d/"+clas+"/"+clas+"_" + dataType + "_embed_" + language + ".sent"
                    indexFile = cnnOutputPath+str(wordDimension)+"d/"+clas+"/"+clas+"_" + dataType + "_index_" + language + ".docm"
                    documentVectorFile = cnnOutputPath+str(wordDimension)+"d/"+clas+"/"+clas+"_" + dataType + "_embed_" + language +".docm"
                    gendocumentVector(numberFile, fragmentVectorFile, indexFile, documentVectorFile, mDimension)