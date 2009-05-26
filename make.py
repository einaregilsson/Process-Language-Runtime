#!/usr/bin/python
# $Id: make.py 28 2008-11-15 21:33:54Z eboeg $
#
# Utility to delete .pyc files, add svn properties to new files and ensure
# all files start with the correct header.

import sys, os
from os.path import join, split, exists

BASE_FOLDER = split(__file__)[0]

    
def headers(filename):
    file = open(filename)
    text = file.read()
    file.close()
    if not text.startswith("/**"):
        print filename[len(BASE_FOLDER)+1:], 'does not have a correct header, fixing...'
        file = open(filename, 'w')
        file.write(
"""/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
 """)
        file.write(text)
        file.close()

def props(filename):
    name = filename[len(BASE_FOLDER)+1:]
    pipe = os.popen('svn propget svn:keywords "%s"' % filename)
    result = pipe.read()
    pipe.close()
    if result.strip() != 'Id':
        sys.stdout.write(name + ' does not have the svn:keywords Id set, attempting to set now ... ')
        pipe = os.popen('svn propset svn:keywords Id "%s"' % filename)
        result = pipe.read()
        pipe.close()
        if result.strip() == "property 'svn:keywords' set on '%s'" % filename:
            print 'done!'
        else:
            print 'failed!'

def quit(filename):
    sys.exit(0)

def walk(func):
    for root, folders, files in os.walk(BASE_FOLDER):
        if not '.svn' in root and not '\\obj\\' in root:
            for file in files:
                if file.endswith('.cs'):
                    func(join(root, file))
    
if __name__ == '__main__':
    command = ''
    if len(sys.argv) > 1: command = sys.argv[1]
    
    options = {'h' : headers, 'p' : props, 'q' : quit,'headers' : headers, 'props' : props, 'quit' : quit}
    while command not in options.keys():
        print '''
Make utility
Usage: make.py param

where param is one of the following:

    headers : Adds the correct header to .py files that don't have it
    props   : Adds the svn:keyword Id to all .py files that don't have it
    quit    : Quits this program
    '''
        command = raw_input('Enter command (or first letter of command): ')
        
    walk(options[command])

    if len(sys.argv) == 1 or sys.argv[1] not in options.keys():
        raw_input('Press any key to exit')
                    
               