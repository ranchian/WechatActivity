var account = {
    redirect: "",
    /**
     * 获取地址栏参数
     * @param {} name 
     * @returns {} 
     */
    getQueryStr: function (name) {
        var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)");
        var r = window.location.search.substr(1).match(reg);
        if (r != null)
            return unescape(r[2]);
        return null;
    },
    /**
     * 页面跳转
     * @param {} url 
     * @returns {} 
     */
    go: function (url) {
        if (typeof (url) === "undefined") {
            return;
        }
        window.location.href = url + window.location.search;
    },
    /**
     * 登录方法
     * @returns {} 
     */
    login: function () {
        var phone = $("#txt-phone").val();
        if (phone === null || phone === "") {
            alert("请输入手机号");
            $("#txt-phone").focus();
            return false;
        }
        //格式验证
        if (!/^1[3|4|5|7|8][0-9]\d{8}$/.test(phone)) {
            alert("请输入正确的手机号");
            $("#txt-phone").focus();
            return false;
        }
        if ($("#txt-pswd").val() === "") {
            alert("请输入您的登录密码");
            $("#txt-pswd").focus();
            return false;
        }

        var param = {
            Phone: $("#txt-phone").val(),
            Pswd: $("#txt-pswd").val()
        };

        $.ajax({
            type: "post",
            timeout: 18000,
            dataType: "json",
            url: "/activity/member/login",
            data: param,
            //请求成功时执行
            success: function (data) {
                if (data !== null && data !== "" && typeof (data) !== "undefined") {
                    //登录成功,后续业务处理
                    if (data.Success === 1) {
                        account.redirect = account.getQueryStr("redirect");
                        if (account.redirect !== null && account.redirect !== "") {
                            account.go(account.redirect);
                            return;
                        }
                        $(".btn-submit").attr("href", "javascript:account.login()");
                        return;
                    }
                    $(".btn-submit").attr("href", "javascript:account.login()");
                    alert(data.Result);
                    return;
                }
                $(".btn-submit").attr("href", "javascript:account.login()");
                alert("网络堵塞,请稍后重试!");
                return;
            },
            //请求失败遇到异常触发
            error: function () {
                alert("网络堵塞,请稍后重试!");
                $(".btn-submit").attr("href", "javascript:account.login()");
            },
            //完成请求后触发。即在success或error触发后触发
            complete: function (xhr, status) {
                if (status === "timeout") {
                    $(".btn-submit").attr("href", "javascript:account.login()");
                }
            },
            //发送请求前触发
            beforeSend: function () {
                $(".btn-submit").attr("href", "javascript:void(0)");
            }
        });
        return false;
    },
    /**
     * 发送验证码
     * @returns {} 
     */
    getCode: function() {
        var phone = $("#txt-phone").val();
        if (phone === null || phone === "") {
            alert("请输入手机号");
            $("#txt-phone").focus();
            return false;
        }
        //格式验证
        if (!/^1[3|4|5|7|8][0-9]\d{8}$/.test(phone)) {
            $("#txt-phone").focus();
            alert("请输入正确的手机号");
            return false;
        }

        var param = {
            phone: phone,
            type: 1
        };

        var data = {
            M: "GetVCode", D: JSON.stringify(param)
        }

        var time = 60, ele = $(".get-smscode"), btnCode = $(".get-smscode");
        btnCode.prop("href", "javascript:;");

        $.ajax({
            type: "post",
            dataType: "json",
            url: "http://api.fangjinnet.com/api",
            data: JSON.stringify(data),
            //请求成功时执行
            success: function (data) {
                if (data != null) {
                    if (data.s !== 0) {
                        alert(data.es);
                        $("#phone").focus();
                        btnCode.prop("href", "javascript:account.getCode();");
                        return false;
                    }
                    alert("验证码发送成功，请查收");
                    var interId = setInterval(function () {
                        time--;
                        ele.html(time + "秒");
                        if (time === 0) {
                            clearInterval(interId);
                            ele.text("获取验证码");
                            btnCode.prop("href", "javascript:account.getCode();");
                        }
                    }, 1000);
                }
                return false;
            },
            error: function () {
                btnCode.prop("href", "javascript:account.getCode();");
            },
            //完成请求后触发。即在success或error触发后触发
            complete: function (xhr, status) {
                if (status === "timeout") {
                    btnCode.prop("href", "javascript:account.getCode();");
                }
            },
            //发送请求前触发
            beforeSend: function () {
                btnCode.prop("href", "javascript:;");
            }
        });
        return false;
    },
    /**
     * 注册
     * @returns {} 
     */
    regist: function() {
        var phone = $("#txt-phone").val();
        if (phone === null || phone === "") {
            alert("请输入手机号");
            $("#txt-phone").focus();
            return false;
        }
        //格式验证
        if (!/^1[3|4|5|7|8][0-9]\d{8}$/.test(phone)) {
            $("#txt-phone").focus();
            alert("请输入正确的手机号");
            return false;
        }
        if ($("#txt-smscode").val() === "") {
            $("#txt-smscode").focus();
            alert("请输入验证码");
            return false;
        }
        if ($("#txt-pswd").val() === "") {
            $("#txt-pswd").focus();
            alert("请先设置您的登录密码");
            return false;
        }
        var friend = $("#txt-friend").val();
        if (friend != null && friend !== "") {
            if (!/^1[3|4|5|7|8][0-9]\d{8}$/.test(friend)) {
                $("#txt-friend").focus();
                alert("请输入正确的邀请人手机号");
                return false;
            }
        }

        var param = {
            phone: $("#txt-phone").val(),
            code: $("#txt-smscode").val(),
            pswd: $("#txt-pswd").val(),
            inviterPhone: $("#txt-friend").val(),
            channel:""
        };

        $(".btn-submit").attr("href", "javascript:void(0)");
        $.ajax({
            type: "post",
            timeout: 18000,
            dataType: "json",
            url: "/activity/member/regist",
            data: param,
            //请求成功时执行
            success: function (data) {
                if (data !== null && data !== "" && typeof (data) !== "undefined") {
                    //登录成功,后续业务处理
                    if (data.success) {
                        account.redirect = account.getQueryStr("redirect");
                        if (account.redirect !== null && account.redirect !== "") {
                            account.go(account.redirect);
                            return;
                        }
                        $(".btn-submit").attr("href", "javascript:account.regist()");
                        return;
                    }
                    $(".btn-submit").attr("href", "javascript:account.regist()");
                    alert(data.message);
                    return;
                }
                $(".btn-submit").attr("href", "javascript:account.regist()");
                alert("网络堵塞,请稍后重试!");
                return;
            },
            //请求失败遇到异常触发
            error: function () {
                alert("网络堵塞,请稍后重试!");
                $(".btn-submit").attr("href", "javascript:account.regist()");
            },
            //完成请求后触发。即在success或error触发后触发
            complete: function (xhr, status) {
                if (status === "timeout") {
                    $(".btn-submit").attr("href", "javascript:account.regist()");
                }
            },
            //发送请求前触发
            beforeSend: function () {
                $(".btn-submit").attr("href", "javascript:void(0)");
            }
        });
        return false;
    }
};