; (function ($) {
    $.fjwxconfig = function (options, wx) {
        var de = {
            debug: true,
            key: "",
            hideAll: false,
            shareTimeline: true,
            shareAppMessage: true,
            shareQQ: true,
            shareQZone: true,
            shareWeibo: true,
            title: "房金网",
            desc: "房金网",
            url: location.href,
            img: "#"
        }

        var settings = $.extend({}, de, options);
        var list = ["checkJsApi",
            "onMenuShareTimeline",
            "onMenuShareAppMessage",
            "onMenuShareQQ",
            "onMenuShareWeibo",
            "onMenuShareQZone",
            "chooseImage",
            "previewImage",
            "uploadImage",
            "downloadImage",
            "hideOptionMenu",
            "showOptionMenu",
            "hideMenuItems",
            "showMenuItems",
            "hideAllNonBaseMenuItem",
            "showAllNonBaseMenuItem",
            "scanQRCode"
        ];
        var hidemenus = [];

        if (settings.hideAll || !settings.shareAppMessage) {
            hidemenus.push("menuItem:share:appMessage");
        }

        if (settings.hideAll || !settings.shareTimeline) {
            hidemenus.push("menuItem:share:timeline");
        }

        if (settings.hideAll || !settings.shareQQ) {
            hidemenus.push("menuItem:share:qq");
        }

        if (settings.hideAll || !settings.shareQZone) {
            hidemenus.push("menuItem:share:QZone");
        }

        if (settings.hideAll || !settings.shareWeibo) {
            hidemenus.push("menuItem:share:weiboApp");
        }

        $.ajax({
            url: "http://a.fangjinnet.com/Wechat/jsconfig",
            data: {
                url: location.href
            },
            crossDomain:true,
            success: function(resp) {

                if (!resp.success) {
                    debugTips(resp, "jsconfigResult");
                    return;
                }

                wx.config({
                    debug: settings.debug, // 开启调试模式
                    appId: resp.data.appid, // 必填，公众号的唯一标识
                    timestamp: resp.data.timestamp, // 必填，生成签名的时间戳
                    nonceStr: resp.data.nonceStr, // 必填，生成签名的随机串
                    signature: resp.data.signature, // 必填，签名，见附录1
                    jsApiList: list // 必填，需要使用的JS接口列表，所有JS接口列表见附录2
                });

                wx.ready(function() {

                    var d = {
                        title: settings.title, // 分享标题
                        link: settings.url, // 分享链接
                        desc: settings.desc,
                        imgUrl: settings.img, // 分享图标
                        success: function() {
                            // 用户确认分享后执行的回调函数
                        },
                        cancel: function() {
                            // 用户取消分享后执行的回调函数
                        }
                    };
                    if (hidemenus.length > 0) {
                        wx.hideMenuItems({
                            menuList: hidemenus
                        });
                    }
                    if (settings.hideAll) {
                        wx.hideOptionMenu();
                    }
                    if (settings.shareAppMessage) {
                        wx.onMenuShareAppMessage(d);
                    }

                    if (settings.shareTimeline) {
                        wx.onMenuShareTimeline(d);
                    }

                    if (settings.shareQQ) {
                        wx.onMenuShareQQ(d);
                    }

                    if (settings.shareQZone) {
                        wx.onMenuShareQZone(d);
                    }

                    if (settings.shareWeibo) {
                        wx.onMenuShareWeibo(d);
                    }

                    //choose image
                    $(".choose-img").click(function() {
                            var t = $(this).attr("data-type");
                            wx.chooseImage({
                                success: function(res) {
                                    var localIds = res.localIds;
                                    for (var i = 0; i < localIds.length; i++) {

                                        wx.uploadImage({
                                            localId: localIds[i],
                                            isShowProgressTips: 1,
                                            success: function(uploadResp) {
                                                uploadImg(uploadResp.serverId, t);
                                            },
                                            fail: function(fail) {
                                                debugTips(fail);
                                            }
                                        });
                                    }
                                }
                            });
                        });
                    //--


                });
            },
            error: function(ex) {
                console.error(ex);
            }
        });

        function uploadImg(serverIds, type) {
            var d = {
                type: type,
                url: serverIds
            };
            $.ajax({
                url: "http://a.fangjinnet.com/WechatFunction/UploadImg",
                data: d,
                dataType: "json",
                beforeSend: function () {
                    debugTips(d, "data");
                },
                success: function (imgResp) {
                    debugTips(imgResp, "uploadImg");
                },
                error: function (r, s, e) {
                    debugTips({ status : s}, "error");
                }
            });
        }

        function debugTips(obj, tip) {
            if (settings.debug) {
                alert(tip +":" + JSON.stringify(obj));
            }
        }
    }
})(jQuery);

