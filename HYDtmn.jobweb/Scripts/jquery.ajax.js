; (function (asyncLib, $) {
    function getVerifyToken(keyselector) {
        var keyarea = $(document);
        if (keyselector) {
            keyarea = $(keyselector);
        }
        var token = $('input[name="__RequestVerificationToken"]', keyarea).val();
        var headers = {};
        headers['__RequestVerificationToken'] = token;
        return headers;
    }

    function getFormSearchValues(formClass) {
        var form = $(formClass);
        var formvalues = form.serialize();
        return decodeFormParams(formvalues);

    }
    function encodeQueryData(url,data) {
        const ret = [];
        for (let d in data)
            ret.push(encodeURIComponent(d) + '=' + encodeURIComponent(data[d]));
        return url+'?'+ret.join('&');
    }

    function saveFileByEvent(event) {
        if (event == undefined) {
            return;
        } else if (event.type === HttpEventType.DownloadProgress) {
            // in progress
            const percentDone = Math.round(100 * event.loaded / event.total);
            console.log(`File is ${percentDone}% downloaded.`);
        } else if (event instanceof HttpResponse) {
            // process completed
            if (event.body) {
                console.log('File is completely downloaded!');

                var a = document.createElement("a");
                a.setAttribute("style", "display: none");
                var mimetype = event.body.type;//"text/plain";
                //if (this.Format == "PDF")
                //  mimetype = "application/pdf";
                //else if (this.Format == "Word")
                //  mimetype = "application/msword";
                //else if (this.Format == "RichText")
                //  mimetype = "application/rtf";
                var blob = new Blob([event.body], { type: mimetype });
                var url = window.URL.createObjectURL(blob);
                a.href = url;
                var contentDisposition = event.headers.get('content-disposition');
                var filename = contentDisposition.split(';')[1].split('filename')[1].split('=')[1].trim();
                a.download = filename;
                document.body.appendChild(a);
                a.click();
                window.URL.revokeObjectURL(url);
                document.body.removeChild(a);



            }
        }
    }    

    function decodeFormParams(params) {
        var pairs = params.split('&'),
            result = {};

        for (var i = 0; i < pairs.length; i++) {
            var pair = pairs[i].split('='),
                key = decodeURIComponent(pair[0]),
                value = decodeURIComponent(pair[1]),
                isArray = /\[\]$/.test(key),
                dictMatch = key.match(/^(.+)\[([^\]]+)\]$/);


            if (dictMatch) {
                key = dictMatch[1];
                var subkey = dictMatch[2];

                result[key] = result[key] || {};
                result[key][subkey] = value;
            } else if (isArray) {
                key = key.substring(0, key.length - 2);
                result[key] = result[key] || [];
                result[key].push(value);
            } else {
                result[key] = value;
            }
        }

        return result;
    }
    var ajaxCall = function (ajaxoptions, keyselector, dialogmodal, dialogstop) {
        var jload = function () {
            if (dialogmodal && dialogmodal.open) {
                dialogmodal.open();
            }

            return $.ajax($.extend({}, ajaxoptions, { headers: getVerifyToken(keyselector) }));

        }
        var jdone = function (data, needError) {

            dialogstop.resolve();

            if (needError) {

                if (data.errorlist && data.errorlist.length > 0) {
                    console.log(data.errorlist.map(function (e, i) { return e.MemberNames.join(','); }));


                    let msgArray = [];
                    data.errorlist.forEach(function (e, i) {
                        //msgArray.push(e.ErrorMessage);
                        let avail = false;
                        var ctrl = $("[name='" + e.MemberNames[0] + "']");

                        if (ctrl) {
                            var validator = $(ctrl).closest("form").validate();
                            if (validator) {

                                // both control and validator are available
                                avail = true;
                                var msg = {};
                                msg[e.MemberNames[0]] = e.ErrorMessage;
                                validator.showErrors(msg);
                            }
                        }
                        if (false == avail) {
                            msgArray.push(e.ErrorMessage);
                        }
                    });


                    if (msgArray.length > 0) {

                        // unique message
                        let onlyUnique = function (value, index, self) {
                            return self.indexOf(value) === index;
                        }

                        // show all message on the same notifier
                        //CoreLib.Utils.showErrorNotify(msgArray.filter(onlyUnique).map(function (e, i) { return e + "</br>"; }), "", true);
                    }
                    return false;
                }
            }

            if (data.errorlist && data.errorlist.length > 0) {
                return false;
            }

            //setTimeout(dialogstop.resolve, 1000);


            return true;


        };
        var jfail = function (err) {
            dialogstop.resolve();

            // support err in xhr or serialized format
            var errObj = null;
            if (typeof err === 'string' || err instanceof String) {
                errObj = JSON.parse(err);
            }
            else {
                errObj = err;
            }

            if (errObj.status == 408) {
                window.location.reload();
            }

            var errTxt = ((errObj && errObj.responseText) || "Unexpected Error Occurred");
            var errKey = null;

            if (errObj.responseJSON && errObj.responseJSON.Message) {
                errTxt = errObj.responseJSON.Message;
            }
            if (errObj.responseJSON && errObj.responseJSON.ErrorKey) {
                errKey = errObj.responseJSON.ErrorKey;
            }

            //var errmodal = CoreLib.Utils.getModal(errTxt, 'modaldialog-top', 'modaldialog-msg', '<error>', 'rgba(0, 0, 0, 0.5)', '#fff', null);
            //errmodal.open();
            //CoreLib.Utils.showErrorNotify(errTxt, "", true, errKey);

        };
        return {
            jload: jload,
            jdone: jdone,
            jfail: jfail
        }

    }

    var queueajax = function (ajaxoptions, keyselector, spinning, dialogmodal) {
        var dialogstop = $.Deferred();
        var spinned = spinning ? spinning() : null;
        dialogstop.promise().always(function () {
            if (spinned && spinned.stop) {
                spinned.stop();
            }

        });

        return ajaxCall(ajaxoptions, keyselector, dialogmodal, dialogstop);
    }
    $.ajaxQueue = queueajax;


    asyncLib.Core = {
        getVerifyToken: getVerifyToken,
        ajaxCall: ajaxCall,
        getFormSearchValues: getFormSearchValues,
        saveFileByEvent: saveFileByEvent,
        encodeQueryData: encodeQueryData
    }


})(this.asyncLib = this.asyncLib || {}, jQuery);