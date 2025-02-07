/* Author：mingyuhisoft@163.com
 * Github:https://github.com/imingyu/jquery.mloading
 * Npm:npm install jquery.mloading.js
 * Date：2016-7-4
 */

;(function (root, factory) {
    'use strict';

    if (typeof module === 'object' && typeof module.exports === 'object') {
        factory(require('jquery'),root);
    } if(typeof define ==="function"){
        if(define.cmd){
            define(function(require, exports, module){
                var $ = require("jquery");
                factory($,root);
            });
        }else{
            define(["jquery"],function($){
                factory($,root);
            });
        }
    }else {
        factory(root.jQuery,root);
    }
} (typeof window !=="undefined" ? window : this, function ($, root, undefined) {
    'use strict';
    if(!$){
        $ = root.jQuery || null;
    }
    if(!$){
        throw new TypeError("必须引入jquery库方可正常使用！");
    }

    var arraySlice = Array.prototype.slice,
        comparison=function (obj1,obj2) {
            var result=true;
            for(var pro in obj1){
                if(obj1[pro] !== obj2[obj1]){
                    result=true;
                    break;
                }
            }
            return result;
        }

    function MLoading(dom,options) {
        options=options||{};
        this.dom=dom;
        this.options=$.extend(true,{},MLoading.defaultOptions,options);
        this.curtain=null;
        this.render().show();
    }
    MLoading.prototype={
        constructor:MLoading,
        initElement:function () {
            var dom=this.dom,
                ops=this.options;
            var curtainElement=dom.children(".mloading"),
                bodyElement = curtainElement.children('.mloading-body'),
                barElement = bodyElement.children('.mloading-bar'),
                iconElement = barElement.children('.mloading-icon'),
                textElement = barElement.find(".mloading-text");
            if (curtainElement.length == 0) {
                curtainElement = $('<div class="mloading"></div>');
                dom.append(curtainElement);
            }
            if (bodyElement.length == 0) {
                bodyElement = $('<div class="mloading-body"></div>');
                curtainElement.append(bodyElement);
            }
            if (barElement.length == 0) {
                barElement = $('<div class="mloading-bar"></div>');
                bodyElement.append(barElement);
            }
            if (iconElement.length == 0) {
                var _iconElement=document.createElement(ops.iconTag);
                iconElement = $(_iconElement);
                iconElement.addClass("mloading-icon");
                barElement.append(iconElement);
            }
            if (textElement.length == 0) {
                textElement = $('<span class="mloading-text"></span>');
                barElement.append(textElement);
            }
            
            this.curtainElement=curtainElement;
            this.bodyElement = bodyElement;
            this.barElement = barElement;
            this.iconElement = iconElement;
            this.textElement = textElement;
            return this;
        },
        render:function () {
            var dom=this.dom,
                ops=this.options;
            this.initElement();
            if(dom.is("html") || dom.is("body")){
                this.curtainElement.addClass("mloading-full");
            }else{
                this.curtainElement.removeClass("mloading-full");

                if(!dom.hasClass("mloading-container")){
                    dom.addClass("mloading-container");
                }
            }
            if(ops.mask){
                this.curtainElement.addClass("mloading-mask");
            }else{
                this.curtainElement.removeClass("mloading-mask");
            }
            if(ops.content!="" && typeof ops.content!="undefined"){
                if(ops.html){
                    this.bodyElement.html(ops.content);
                }else{
                    this.bodyElement.text(ops.content);
                }
            }else{
                this.iconElement.attr("src",ops.icon);
                if(ops.html){
                    this.textElement.html(ops.text);
                }else{
                    this.textElement.text(ops.text);
                }
            }

            return this;
        },
        setOptions:function (options) {
            options=options||{};
            var oldOptions = this.options;
            this.options = $.extend(true,{},this.options,options);
            if(!comparison(oldOptions,this.options)) this.render();
        },
        show:function () {
            var dom=this.dom,
                ops=this.options,
                barElement=this.barElement;
            this.curtainElement.addClass("active");
            barElement.css({
                "marginTop":"-"+barElement.outerHeight()/2+"px",
                "marginLeft":"-"+barElement.outerWidth()/2+"px"
            });

            return this;
        },
        hide:function () {
            var dom=this.dom,
                ops=this.options;
            this.curtainElement.removeClass("active");
            if(!dom.is("html") && !dom.is("body")){
                dom.removeClass("mloading-container");
            }
            return this;
        },
        destroy:function () {
            var dom=this.dom,
                ops=this.options;
            this.curtainElement.remove();
            if(!dom.is("html") && !dom.is("body")){
                dom.removeClass("mloading-container");
            }
            dom.removeData(MLoading.dataKey);
            return this;
        }
    };
    MLoading.dataKey="MLoading";
    MLoading.defaultOptions = {
        text:"Loading...",
        iconTag:"img",
        icon:"data:image/gif;base64,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",
        html:false,
        content:"",//设置content后，text和icon设置将无效
        mask:true//是否显示遮罩（半透明背景）
    };

    $.fn.mLoading=function (options) {
        var ops={},
            funName="",
            funArgs=[];
        if(typeof options==="object"){
            ops = options;
        }else if(typeof options ==="string"){
            funName=options;
            funArgs = arraySlice.call(arguments).splice(0,1);
        }
        return this.each(function (i,element) {
            var dom = $(element),
                plsInc=dom.data(MLoading.dataKey);
            if(!plsInc){
                plsInc=new MLoading(dom,ops);
            }

            if(funName){
                var fun = plsInc[funName];
                if(typeof fun==="function"){
                    fun.apply(plsInc,funArgs);
                }
            }
        });
    }
}));