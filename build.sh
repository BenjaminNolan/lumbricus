#!/bin/bash
xbuild /p:Configuration=Debug Lumbricus.sln
for i in `ls -d Plugins/*`; do
	cp Plugins/`basename $i`/bin/Debug/`basename $i`.dll* Lumbricus/bin/Debug/plugins/
done

