#!/bin/bash
git submodule update --init
echo "namespace BaggyBot{public sealed partial class Bot{public static string Version =>\""`git describe --tags`"\";}}" > ./baggybot/src/Version.cs
