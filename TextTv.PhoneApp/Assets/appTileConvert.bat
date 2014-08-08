convert AppTile.png -resize "170x170>" SquareLogo.scale-240.png
convert AppTile.png -resize "99x99>" SquareLogo.scale-140.png
convert AppTile.png -resize "71x71>" SquareLogo.scale-100.png

convert AppTile.png -resize "360x360>" Logo.scale-240.png
convert AppTile.png -resize "210x210>" Logo.scale-140.png
convert AppTile.png -resize "150x150>" Logo.scale-100.png

convert AppTile.png -resize "744x360>" -size 744x360 xc:transparent +swap -gravity center -composite WideLogo.scale-240.png
convert AppTile.png -resize "434x210>" -size 434x210 xc:transparent +swap -gravity center -composite WideLogo.scale-140.png
convert AppTile.png -resize "310x150>" -size 310x150 xc:transparent +swap -gravity center -composite WideLogo.scale-100.png

convert AppTile.png -resize "106x106>" SmallLogo.scale-240.png
convert AppTile.png -resize "62x62>" SmallLogo.scale-140.png
convert AppTile.png -resize "44x44>" SmallLogo.scale-100.png

convert AppTile.png -resize "120x120>" StoreLogo.scale-240.png
convert AppTile.png -resize "70x70>" StoreLogo.scale-140.png
convert AppTile.png -resize "50x50>" StoreLogo.scale-100.png

convert AppTile.png -resize "58x58>" BadgeLogo.scale-240.png
convert AppTile.png -resize "33x33>" BadgeLogo.scale-140.png
convert AppTile.png -resize "24x24>" BadgeLogo.scale-100.png

convert AppTile.png -resize "1152x1920>" -size 1152x1920 xc:transparent +swap -gravity center -composite SplashScreen.scale-240.png
convert AppTile.png -resize "672x1120>" -size 672x1120 xc:transparent +swap -gravity center -composite SplashScreen.scale-140.png
convert AppTile.png -resize "480x800>" -size 480x800 xc:transparent +swap -gravity center -composite SplashScreen.scale-100.png
