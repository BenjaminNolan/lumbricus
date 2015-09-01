#!/bin/bash

mono --debug Lumbricus/bin/Debug/Lumbricus.exe | tee logs/Lumbricus.`date +%Y-%m-%d_%H.%M.%S`.log

