git submodule add --force https://github.com/dayasui/CommonTest.git Assets/ParallelCommon
git commit -m "add module"
cd Assets/ParallelCommon
git config core.sparsecheckout true
echo /Assets/ParallelCommon/ > ../../.git/modules/Assets/ParallelCommon/info/sparse-checkout
git read-tree -mu HEAD

