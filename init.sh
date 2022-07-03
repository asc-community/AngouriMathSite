cd _generator
git clone https://github.com/asc-community/AngouriMath AngouriMath
git clone https://github.com/WhiteBlackGoose/Yadg.NET Yadg.NET
cd content
git clone https://github.com/asc-community/AngouriMath.wiki.git _wiki
cd ../AngouriMath/Sources/AngouriMath/AngouriMath
dotnet build -c release  
