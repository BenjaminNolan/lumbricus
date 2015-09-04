#!/bin/bash
nuget restore
xbuild /p:Configuration=Debug Lumbricus.sln
if [ ! -d Lumbricus/bin/Debug/plugins ]; then
	mkdir -p Lumbricus/bin/Debug/plugins
fi
for i in `ls -d Plugins/*`; do
	cp Plugins/`basename $i`/bin/Debug/`basename $i`.dll* Lumbricus/bin/Debug/plugins/
done
