; (function (alertLib, $, BpD) {

    function askYesNo(title, yesMsg, NoAns, captureData) {
        var $df = $.Deferred();
        var label = typeof yesMsg === 'function' ? yesMsg() : yesMsg;
        if (BpD.askconfirm) {
            delete BpD.askconfirm;
        }
        if (!NoAns) {
            NoAns = 'No';
        }

        BpD.dialog('askconfirm', function factory() {
            return {
                main: function (message) {
                    this.message = message;
                },
                setup: function () {
                    return {
                        buttons: [{ text: 'Confirm', className: 'matter-button-contained btn-green', invokeOnClose: true,attrs: { name: 'confirmdg'} },
                        { text: NoAns, className: 'matter-button-contained btn-grey', invokeOnClose: true,attrs: {name: 'canceldg'} }],
                        focus: { element: 1 },
                        options: {
                            title: title,
                            maximizable: false,
                            closableByDimmer: false,
                            mode: captureData && captureData.mode ? captureData.mode: undefined


                        }

                    }
                },
                prepare: function () {
                    this.setContent(this.message);
                },
                callback: function (closeEvent) {
                    if (closeEvent.index == 0) {
                        $df.resolve({ status: 'yes', yes: true, data: captureData, event: closeEvent });
                    }
                    else {
                        $df.resolve({ status: 'no', no: true, data: captureData, event: closeEvent });
                    }

                }
            }
        }, true);
        var confirmInstance = BpD.askconfirm(label);
        if(captureData && captureData.reuse)
        {
            captureData.instance = confirmInstance;
        }

        return $df;


    }

    function alert(title, yesMsg, yesbtn, captureData) {
        var label = typeof yesMsg === 'function' ? yesMsg() : yesMsg;
        var $df = $.Deferred();
        if (BpD.alertme) {
            delete BpD.alertme;
        }
        BpD.dialog('alertme', function factory() {
            return {
                main: function (message) {
                    this.message = message;
                },
                setup: function () {
                    return {
                        buttons: [{ text: yesbtn, className: 'matter-button-contained btn-green', invokeOnClose: true }],
                        focus: { element: 0 },
                        options: {
                            title: title,
                            maximizable: false,
                            closableByDimmer: false,
                            mode: captureData && captureData.mode ? captureData.mode: undefined,


                        }

                    }
                },
                prepare: function () {
                    this.setContent(this.message);
                },
                callback: function (closeEvent) {
                    $df.resolve({ status: 'yes', yes: true, data: captureData, event: closeEvent })
                }
            }
        }, true);
        BpD.alertme(label);
        return $df;

    }








    alertLib.Core = {
        alert: alert,
        askYesNo: askYesNo,


    }

})(this.alertLib = this.alertLib || {}, jQuery, alertify);