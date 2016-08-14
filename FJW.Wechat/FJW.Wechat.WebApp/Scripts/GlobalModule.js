

var Global={
	gname:"ooGold",
	GameBox:document.getElementById("game_box"),
	oHero:document.getElementById("hero"),
	oGameOver:document.getElementById("game_over"),
	oTyj:document.getElementById("tiyanjin"),
	oHeroTop:0,
	oHeroLeft:0,
	oScore:document.getElementById("score"),
	oStopWatch:document.getElementById("stopwatch"),
	box_size:{width:document.documentElement.clientWidth,height:document.documentElement.clientHeight},
	goldall:{},
	timer:null,
	code:0,//计数器
	time:30,
	second:0,
	msec:0,
	stopTime:null	
}
