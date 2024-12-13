; (function (filedropLib, $, $rus) {
    
    const DROP_CLASS = ".resumable-drop";
    const DRAG_CLASS = "resumable-dragover";
    const BWSE_CLASS = ".resumable-browse";
    const PGRS_CLASS = ".resumable-progress";
    const RSME_CLASS = ".progress-resume-link";
    const PUSE_CLASS = ".progress-pause-link";
    const LIST_CLASS = ".resumable-list";
    const ERRO_CLASS = ".resumable-error";
    const PBAR_CLASS = ".progress-bar";
    const CLER_CLASS = ".resumable-clear";
    const INRD_CLASS = "input[type='radio'][name='upload_token']:checked";
    
    const TOKE_ATTRS = "thumbkey"; 
    
    function getSelectedTokenfn(getwrapper) {
        return function () {
            const tokenvalue = $(getwrapper()+INRD_CLASS).val();
            $(getwrapper()+BWSE_CLASS).attr(TOKE_ATTRS, $(getwrapper()+INRD_CLASS).val());
            return { 'upload_token': tokenvalue };
        };
    }
    function setupClear(r, getinputselector,getwrapper) {
        console.log('inputselector is of function type = '+ (typeof getinputselector == 'function'));
        $(getwrapper()+CLER_CLASS).off('click');
        $(getwrapper()+CLER_CLASS).click(function (e) {
            var $clrbtn = $(e.target);
            $(getwrapper()+LIST_CLASS).children().remove();
            r.files = [];

            var subval = $(getwrapper()+'input[type="submit"').attr(TOKE_ATTRS);

            var inputtokenid = $(getwrapper()+INRD_CLASS).attr('id');
            var thumbkey = inputtokenid.replace('token-', '');
            if (thumbkey) {
                $(getwrapper()+PBAR_CLASS)[0].style.removeProperty("width");
                //to clear the input value according to tokenid
                $(getinputselector('[tokenup="' + inputtokenid + '"]')).val('-');
                var $displayitems =$('span[tokenup="' + inputtokenid + '"]').next(getwrapper()+'.resumable-thumb').find('video,img');
                $displayitems.attr('src', '');

                $displayitems.each(function(i,e){
                    var $el = $(e);
                    if($el.is('video'))
                    {
                        $el.addClass('hidden');
                    }
                    else{
                        $el.removeClass('hidden');
                    }
                });

                if(subval)
                {
                    $(getwrapper()+'input[type="submit"').attr(TOKE_ATTRS, subval.replace(thumbkey, ""));

                }
                
            }

        });
    }
    function setupPic(getinputselector,basename,file,getwrapper) {
        console.log('inputselector is of function type = ' + (typeof getinputselector == 'function'));
        const inputtokenid = $(getwrapper()+INRD_CLASS).attr('id');
        const thumbkey = inputtokenid.replace('token-', '');
        if (thumbkey) {
            $(getinputselector('[tokenup="' + inputtokenid + '"]')).val($(getwrapper()+INRD_CLASS).val());
            let $video = $('span[tokenup="' + inputtokenid + '"]').next(getwrapper()+'.resumable-thumb').find('video');
            let $img = $('span[tokenup="' + inputtokenid + '"]').next(getwrapper()+'.resumable-thumb').find('img');
            if (basename.endsWith(".mp4")) {
                var fileUrl = window.URL.createObjectURL(file.file);
                $video.removeClass('hidden');
                $video.attr("src", fileUrl);
                $img.addClass('hidden');

            }
            else {
                $img.removeClass('hidden');
                $video.addClass('hidden');
                var reader = new FileReader();
                reader.onload = function () {
                    var output = $img[0];
                    output.src = reader.result;
                };
                
                reader.readAsDataURL(file.file);

            }
            var subval = $(getwrapper()+'input[type="submit"').attr(TOKE_ATTRS);
            if (!subval)
                subval = '';

            $(getwrapper()+'input[type="submit"').attr(TOKE_ATTRS, subval+thumbkey);
        }



    }
    function setup(getinputselector,getwrapper) {

        $(getwrapper()+DROP_CLASS).off('dragenter');
        $(getwrapper()+DROP_CLASS).on('dragenter', function () { $(this).addClass(DRAG_CLASS); });
        $(getwrapper()+DROP_CLASS).off('dragend');
        $(getwrapper()+DROP_CLASS).on('dragend', function () { $(this).removeClass(DRAG_CLASS); });
        $(getwrapper()+DROP_CLASS).off('drop');
        $(getwrapper()+DROP_CLASS).on('drop', function () { $(this).removeClass(DRAG_CLASS); });

        var uplurl = $(getwrapper()+BWSE_CLASS).attr('urlis');
        var r = new $rus({
            target: uplurl,
            query: getSelectedTokenfn(getwrapper),
            chunkSize: 1 * 1024 * 1024,
            simultaneousUploads: 4,
            fileType: ['jpg', 'png', 'mp4'],
            testChunks: true,
            throttleProgressCallbacks: 1
        });

        $(getwrapper()+PGRS_CLASS + ' ' + RSME_CLASS).off('click');
        $(getwrapper()+PGRS_CLASS + ' ' + RSME_CLASS).click(function () {
            r.upload(); return false;
        });
        $(getwrapper()+PGRS_CLASS + ' ' + PUSE_CLASS).off('click');
        $(getwrapper()+PGRS_CLASS + ' ' + PUSE_CLASS).click(function () {
            r.pause(); return false;
        });

        setupClear(r, getinputselector,getwrapper);


        // Resumable.js isn't supported, fall back on a different method
        if (!r.support) {
            $(getwrapper()+ERRO_CLASS).show();
        } else {
            // Show a place for dropping/selecting files
            $(getwrapper()+DROP_CLASS).show();
            
            r.assignDrop($(getwrapper()+DROP_CLASS)[0]);
            r.assignBrowse($(getwrapper()+BWSE_CLASS)[0]);

            // Handle file add event

            r.on('fileAdded', function (file) {
                // Show progress pabr
                $(getwrapper()+PGRS_CLASS + ',' +getwrapper()+ LIST_CLASS).show();
                // Show pause, hide resume
                $(getwrapper()+PGRS_CLASS + ' ' + RSME_CLASS).hide();
                $(getwrapper()+PGRS_CLASS + ' ' + PUSE_CLASS).show();
                // Add the file to the list
                $(getwrapper()+LIST_CLASS).append('<li class="resumable-file-' + file.uniqueIdentifier + '">Uploading <span class="resumable-file-name"></span> <span class="resumable-file-progress"></span>');
                $(getwrapper()+'.resumable-file-' + file.uniqueIdentifier + ' .resumable-file-name').html(file.fileName);
                // Actually start the upload
                r.upload();
            });
            r.on('pause', function () {
                // Show resume, hide pause
                $(getwrapper()+PGRS_CLASS + ' ' + RSME_CLASS).show();
                $(getwrapper()+PGRS_CLASS + ' ' + PUSE_CLASS).hide();
            });
            r.on('complete', function () {
                // Hide pause/resume when the upload has completed
                //$('.resumable-progress .progress-resume-link, .resumable-progress .progress-pause-link').hide();
                $(getwrapper()+PGRS_CLASS + ' ' + RSME_CLASS).hide();
                $(getwrapper()+PGRS_CLASS + ' ' + PUSE_CLASS).hide();
            });

            //NOTIFY: PLEASE HELP TO CONTINUE THE CHANGE//TOMORROW

            r.on('fileSuccess', function (file, message) {
                // Reflect that the file upload has completed
                var basename = file.file.name.split('\\/').pop();
                // the browse click token is key to upload , so random number is used in actual upload
                var keyvalue = $(getwrapper()+BWSE_CLASS).attr(TOKE_ATTRS);
                setupPic(getinputselector, basename,file,getwrapper);

                $(getwrapper()+'.resumable-file-' + file.uniqueIdentifier + ' .resumable-file-progress').html('(completed)');
            });
            r.on('fileError', function (file, message) {
                // Reflect that the file upload has resulted in error
                $(getwrapper()+'.resumable-file-' + file.uniqueIdentifier + ' .resumable-file-progress').html('(file could not be uploaded: ' + message + ')');
            });
            r.on('fileProgress', function (file) {
                // Handle progress for both the file and the overall upload
                $(getwrapper()+'.resumable-file-' + file.uniqueIdentifier + ' .resumable-file-progress').html(Math.floor(file.progress() * 100) + '%');
                $(getwrapper()+PBAR_CLASS).css({ width: Math.floor(r.progress() * 100) + '%' });
            });
        }

    }

    filedropLib.Core = {
        setup: setup
    };

    return filedropLib;


})(this.filedropLib = this.filedropLib || {}, jQuery, Resumable);

