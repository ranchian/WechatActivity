
//获取金币坐标
function SetPositionY() {
    var pos = {
        x: parseInt(Math.random() * (Global.box_size.width - 30)),
        y: 0
    }
    return pos
}
function SetPositionX() {
    var pos = {
        x: 0,
        y: parseInt(Math.random() * (Global.box_size.height - 30))
    }
    return pos
}
//设置速度
function SetSpeed() {
    var iSpeed = parseInt(Math.random() * 4 + 4)
    return iSpeed
}
//设置病毒id
function SetID() {
    var str = "qwertyuiopasdfghjklzxcvbnm1234567890QWERTYUIOPASDFGHJKLZXCVBNM"
    var id = "a"
    for (var i = 0; i < 15; i++) {
        id += str[Math.floor(Math.random() * 62)]
    }
    return id
}
//秒表
Global.oStopWatch.innerHTML = "00:00";
function stopWatch() {
    Global.stopTime = setInterval(function startIt() {
        Global.msec++;
        if (Global.msec >= 100) {
            Global.msec = 0;
            Global.second++
        }
        var j = function (arg) {
            return arg >= 10 ? arg : "0" + arg;
        }
        Global.oStopWatch.innerHTML = j(Global.second) + ":" + j(Global.msec); //更新显示
        Global.oTyj.innerHTML = (Global.second * 100) + +j(Global.msec) + '元';
        localStorage.setItem('tiyanjin', Global.oTyj.innerHTML)
    }, 10)
    return Global.oStopWatch.innerHTML
}

//创建病毒
function CreateGold() {
    var id = SetID();
    Global.goldall[id] = new Gold({
        id: id,
        posY: SetPositionY(),
        posX: SetPositionX(),
        speedY: SetSpeed(),
        speedX: SetSpeed(),
        speedx: SetSpeed(),
        speedy: SetSpeed(),
    })
    Global.goldall[id].create();
}

//游戏开始
function GamePlay() {
    stopWatch()
    Global.timer = setInterval(function () {
        Global.code++
        if (Global.code % 40 == 0) {
            CreateGold()
        }
        if (Global.second == 60) {
            Global.oStopWatch.innerHTML = "60" + ":" + "00";
            Global.oTyj.innerHTML = 6000 + '元';
            clearInterval(Global.stopTime);
            clearInterval(Global.timer);
            Global.oGameOver.style.display = "block";
            fnOver("pm25", Global.second * 100 + Global.msec, fid);
        }
        for (var i in Global.goldall) {
            Global.goldall[i].move();
        }
    }, 50);
}


