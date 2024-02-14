#!/usr/bin/env bash

set -e

pushd Thirdparty/love/
cmake -S. -Bbuild
cmake --build build --parallel 8
popd

mkdir -p Thirdparty/Love2dCS/native_lib/usr/lib/
ldd Thirdparty/love/build/love | grep "=> /" | awk '{print $3}' | xargs -I '{}' cp -v '{}' Thirdparty/Love2dCS/native_lib/usr/lib/

pushd Thirdparty/Love2dCS/native_lib/
rm native_lib_linux_x64.zip 
zip -r native_lib_linux_x64.zip ./usr/lib/
rm -rf ./usr/lib
popd
