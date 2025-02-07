; (function (bstrapLib, $, BpD) {
    function setAlert(prefix, messg,delay,toreload) {
        //prefix == '.letter-'
        //messg == 'success'
        if (!delay) {
            delay = 1000;
        }
            

        $(".alert" + prefix + messg).removeClass('hidden');
        window.setTimeout(function () {

            $(".alert" + prefix + messg).fadeTo(delay*1.5, 0).slideUp(delay*1.5, function () {
                
                if (toreload) {
                    setTimeout(function () { location.reload(true); }, delay*0.1);
                }
                
            });
        }, delay);
    }

    function askGoCancel(title, fnmsgcontainer, YesbtnTxt, NobtnTxt,onshown,tobr ) {
        var $df = $.Deferred();
        BpD.show({
            title: title,
            type: BootstrapDialog.TYPE_DEFAULT,
            message: fnmsgcontainer,
            nl2br:tobr,
            cssClass: 'askGo-dialog',
            onshow: function (d) {
                
                if (onshown) {
                    onshown(d);
                }
            },
            buttons: [{
                id: 'yesbtn',
                label: YesbtnTxt,
                cssClass: 'btn btn-default',
                action: function (dialog) {
                    dialog.close();
                    $df.resolve({ status: 'yes', yes: true, dg: dialog.getModal()  });
                }
            },
            {
                id: 'nobtn',
                label: NobtnTxt,
                cssClass: 'btn btn-secondary',

                action: function (dialog) {
                    dialog.close();
                    $df.resolve({ status: 'no', no: true  });
                }
            },

            ]



        });

        return $df;
    }

    function askYesNoGo(title,fnmsgcontainer,YesbtnTxt,NobtnTxt,gobtnTxt,captureData){
        var $df = $.Deferred();
        BpD.show({
            title: title,
            type: BootstrapDialog.TYPE_WARNING,
            message: fnmsgcontainer,
            buttons: [{
                id: 'yesbtn',
                label: YesbtnTxt,
                cssClass: 'btn btn-default',
                action: function (dialog) {
                    dialog.close();
                    $df.resolve({ status: 'yes', yes:true,data: captureData });
                }
            },
                {
                    id: 'nobtn',
                    label: NobtnTxt,
                    cssClass: 'btn btn-secondary',

                    action: function (dialog) {
                        dialog.close();
                        $df.resolve({ status: 'no',no:true,data: captureData });
                    }
                },

                {
                    id: 'gobtn',
                    label: gobtnTxt,
                    cssClass: 'btn btn-dark',
                    action: function (dialog) {
                        dialog.close();
                        $df.resolve({ status: 'go',go:true,data: captureData });
                    }
                },                


            ]



        });

        return $df;
    }

    function infoDg(title, yesMsg, yesbtn,captureData) {
        var $df = $.Deferred();
        BpD.show({
            title: title,
            type: BootstrapDialog.TYPE_INFO,
            message: function (dia) {
                if (typeof yesMsg == - 'function') {
                    return yesMsg();
                }
                return yesMsg;
            },
            buttons: [{
                id: 'yesbtn',
                label: yesbtn,
                cssClass: 'btn btn-dark',
                action: function (dialog) {
                    dialog.close();
                    $df.resolve({ status: 'yes', yes: true, data: captureData });
                }
            }]

        });
        return $df;
    }

    function askYesNo(title, yesMsg, NoAns, captureData) {
        var $df = $.Deferred();

        var NoBtnText = NoAns;
        if (!NoBtnText) {
            NoBtnText = "No";
        }

        BpD.show({
            title: title,
            type: BootstrapDialog.TYPE_DEFAULT,

            message: function (dia) {
                if (typeof yesMsg === 'function') {
                    return yesMsg();

                }
                return yesMsg;

            },


            buttons: [{
                id: 'yesbtn',
                label: 'Yes',
                action: function (dialog) {
                    dialog.close();
                    $df.resolve({ status: 'yes', yes: true, data: captureData });
                }
            },
            {
                id: 'nobtn',
                label: NoBtnText,
                action: function (dialog) {
                    dialog.close();
                    $df.resolve({ status: 'no', no: true, data: captureData });
                }
            }
            ]


        });

        return $df;


    }


    bstrapLib.Core = {
        askYesNo: askYesNo,
        setAlert:setAlert,
        askYesNoGo: askYesNoGo,
        askGoCancel: askGoCancel,
        infoDg: infoDg,

	}

})(this.bstrapLib = this.bstrapLib || {}, jQuery, BootstrapDialog);