#!/bin/bash

xbuild /p:Configuration=Release_FREE;
xbuild /p:Configuration=Release_LITE;
xbuild /p:Configuration=Release_FULL;
