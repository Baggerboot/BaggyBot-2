#!/bin/bash
echo "namespace BaggyBot{public sealed partial class Bot{public static string Version =>\""`git describe --tags`"\";}}" >> ./baggybot/src/Version.cs

xbuild /p:Configuration=Release baggybot-mono.sln
