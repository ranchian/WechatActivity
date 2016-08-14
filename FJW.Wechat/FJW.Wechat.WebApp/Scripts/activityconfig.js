
/*
开始玩
@gamekey: 游戏名
@friendid:帮好友玩的id
*/
function fnPlay( gamekey, friendid ) {
    $.post("/Activity/Default/Play", { key: gamekey, fid: friendid }, function (data) {
        console.log(data);
    });
}

/*
结束
@gamekey: 游戏名
@score: 分数
@friendid:帮好友玩的id
*/
function fnOver(gamekey, score, friendid) {
    $.post("/Activity/Default/Over", { key: gamekey, score: score, fid: friendid }, function (data) {
        console.log(data);
    });
}

/*
领取
@gamekey: 游戏名
@friendid:帮好友玩的id
*/
function fnReceive(gamekey, friendid) {
    $.post("/Activity/Default/Receive", { key: gamekey, fid: friendid }, function (data) {
        console.log(data);
    });
}

/*
获取url参数
@参数名
*/
function getQueryString(name) {
    var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)", "i");
    var r = window.location.search.substr(1).match(reg);
    if (r != null) return unescape(r[2]); return null;
}