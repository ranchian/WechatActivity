function Gold(attr) {
    this.config = {
        id: "",
        speedY: "",
        speedX: "",
        speedx: "",
        speedy: "",
        posY: { x: 10, y: 100 },
        posX: { x: 30, y: 100 }
    }
    for (var i in attr) {
        this.config[i] = attr[i]
    }
}
Gold.prototype = {
    create: function () {
        this.div = document.createElement("li");
        this.li = document.createElement("p");
        this.div.className = "gold";
        this.li.className = "bomb";
        if (this.div.className == "gold") {
            this.div.style.cssText = "top:0;left:" + this.config.posY.x + "px";
            this.div.style.background = "url(images/liyue_bomb" + parseInt(Math.random() * 5) + ".png)"
            this.div.style.backgroundSize = "100% 100%";
            this.div.style.width = "30px"
            this.div.style.height = "30px"
            this.div.style.position = "absolute"
        }
        if (this.li.className == "bomb") {
            this.li.style.cssText = "top:" + this.config.posX.y + "px;left:0";
            this.li.style.background = "url(images/liyue_bomb" + parseInt(Math.random() * 5) + ".png)"
            this.li.style.backgroundSize = "100% 100%";
            this.li.style.width = "30px"
            this.li.style.height = "30px"
            this.li.style.position = "absolute"
        }
        Global.GameBox.appendChild(this.div);
        Global.GameBox.appendChild(this.li);
    },
    move: function () {
        this.config.posY.y += this.config.speedY;
        this.config.posY.x += this.config.speedX;
        this.div.style.top = this.config.posY.y + "px";
        this.div.style.left = this.config.posY.x + "px";
        //碰撞检测
        if (this.config.posY.y >= Global.box_size.height - this.div.offsetHeight) {
            this.config.speedY *= -1
            this.config.posY.y == Global.box_size.height - this.div.offsetHeight
        } else if (this.config.posY.y <= 0) {
            this.config.speedY *= -1
        }
        if (this.config.posY.x >= Global.box_size.width - this.div.offsetWidth) {
            this.config.speedX *= -1;
            this.config.posY.x = Global.box_size.width - this.div.offsetWidth
        } else if (this.config.posY.x <= 0) {
            this.config.speedX *= -1;
        }
        this.config.posX.x += this.config.speedx;
        this.config.posX.y += this.config.speedy;
        this.li.style.left = this.config.posX.x + "px";
        this.li.style.top = this.config.posX.y + "px"
        if (this.config.posX.x >= Global.box_size.width - this.li.offsetWidth) {
            this.config.speedx *= -1;
            this.config.posX.x = Global.box_size.width - this.li.offsetWidth
        } else if (this.config.posX.x <= 0) {
            this.config.speedx *= -1;
        }
        if (this.config.posX.y >= Global.box_size.height - this.li.offsetHeight) {
            this.config.speedy *= -1;
            this.config.posX.y >= Global.box_size.height - this.li.offsetHeight
        } else if (this.config.posX.y <= 0) {
            this.config.speedy *= -1;
        }
        this.drawListener()
    },
    drawListener: function () {
        var goldPosY = { y: this.config.posY.y, x: this.config.posY.x }//病毒的top和left
        var goldPosX = { y: this.config.posX.y, x: this.config.posX.x }
        var goldCenterX = goldPosY.x + 15;//病毒中心点横坐标
        var goldCenterY = goldPosY.y + 15;//病毒中心点纵坐标
        var bombCenterX = goldPosX.x + 15;
        var bombCenterY = goldPosX.y + 15;
        var oHeroCenterX = parseInt(Global.oHeroLeft + Global.oHero.offsetWidth / 2)//运动员中心点横坐标
        var oHeroCenterY = parseInt(Global.oHeroTop + Global.oHero.offsetHeight / 2)//运动员中心点纵坐标
        if (Math.abs(oHeroCenterX - goldCenterX) < (Global.oHero.offsetWidth / 2 + 15) && Math.abs(oHeroCenterY - goldCenterY) < (Global.oHero.offsetHeight / 2 + 15)) {
            clearInterval(Global.stopTime)
            clearInterval(Global.timer);
            Global.oGameOver.style.display = "block";
            Global.oHero.removeEventListener("touchmove", myFunction);
            fnOver("pm25", Global.second * 100 + Global.msec, fid);
        }
        if (Math.abs(oHeroCenterX - bombCenterX) < (Global.oHero.offsetWidth / 2 + 15) && Math.abs(oHeroCenterY - bombCenterY) < (Global.oHero.offsetHeight / 2 + 15)) {
            clearInterval(Global.stopTime)
            clearInterval(Global.timer);
            Global.oGameOver.style.display = "block";
            Global.oHero.removeEventListener("touchmove", myFunction);
            fnOver("pm25", Global.second * 100 + Global.msec, fid);
        }
    }
}