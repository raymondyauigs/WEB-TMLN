function hideshowSection(choiceselector, ishide) {
    var sectionlist = [];
    $(choiceselector).each(function (i, v) {
        if ($(v).is('textarea') || $(v).is('input')) {
            $(v).prop('required', !ishide);

            let vparent = $(v).parent();
            let max = 4;
            do {
                vparent = $(vparent).parent();
                if ($(vparent).is('div[class^="mdc-layout-grid"]')) {

                    ishide && $(vparent).hide(), !ishide && $(vparent).show(), !ishide && $(vparent).siblings('label').show();
                    if (vparent) {
                        var ele = uicontrolLib.Core.isElement(vparent) ? vparent : vparent[0];
                        sectionlist.push(ele);
                    }
                        

                    

                    break;

                }
                max--;
            }
            while (max >= 0);
        }
        else {
            ishide && $(v).hide(), !ishide && $(v).show();
            $(v).find('select').prop('required', !ishide);

            if ($(v)) {
                var ele = uicontrolLib.Core.isElement(v) ? v : $(v)[0];
                sectionlist.push(ele);
            }
                
            

        }

    });

    return sectionlist;
}

function setupSectionShow(selname, hidehandler, showhandler) {


    $('select[name="' + selname + '"],'+'input[type="radio"][name="'+selname+'"]').off('change.section');
    $('select[name="' + selname + '"],' + 'input[type="radio"][name="' + selname + '"]').on('change.section', function (e) {
        let $sel = $(e.target);
        const optval = $sel.val();


        if ($sel.is('select')) {

            $('option:not([value="' + optval + '"])', $sel).each(function (i, el) {
                var $nopt = $(el);
                const noptval = $nopt.val();
                if (noptval) {
                      var showlist = hideshowSection('.' + selname + '-' + noptval, false);
                    
                    if (showhandler) {

                        showhandler(showlist, $sel, optval);
                    }
                }


            });

            const choiceselector = '.' + selname + '-' + optval;
            var hidelist =  hideshowSection(choiceselector, true);
            
            if (hidehandler) {
                hidehandler(hidelist, $sel, optval);
            }

        }
        else if ($sel.is(":checked")) {
            $('input[type="radio"][name="' + $sel.attr('name') + '"]:not([value="' + optval + '"])').each(function (i, el) {
                var $nopt = $(el);
                const noptval = $nopt.val();
                if (noptval) {
                    var showlist = hideshowSection('.' + selname + '-' + noptval, false);
                    

                    
                    if (showhandler) {

                        showhandler(showlist, $sel, optval);
                    }
                }


            });

            const choiceselector = '.' + selname + '-' + optval;
            var hidelist =  hideshowSection(choiceselector, true);
            
            if (hidehandler) {
                hidehandler(hidelist, $sel, optval);
            }
        }




    });
    $('select[name="' + selname + '"],' + 'input[type="radio"][name="' + selname + '"]').trigger('change.section');
}

function removeAnyDisabled(classDisabled) {
    $('input,select', classDisabled).prop('disabled', false);
    $(classDisabled).prop('disabled', false);
    $('.mdc-form-field input:not(:visible)').parents('.mdc-form-field').parent().each(function (i, e) {

        $('*', $(e)).show();
        $(e).show();
    });
}

function setRemoveAnyDisabled(classDisabled) {
    $('form').submit(function (e) {
        removeAnyDisabled(classDisabled);
    });
}

$(document).ready(function () {
    themeLib.Core.setupTheme(null);
    setupSectionShow('IsAdmin', function (hidlist, triggerel, triggerval) {
        if (triggerval == 'True') {
            hidlist.forEach(function (el,i) {
                $(el).find('input[name="IsPowerUser"][type="radio"][value="True"]').prop('checked', true);
            });
            console.log('power user default to full now!');
            
        }
    }, function (shwlist, triggerel, triggerval) {
        if (triggerval == 'False') {
            
            shwlist.forEach(function (el, i) {
                $(el).find('input[name="IsPowerUser"][type="radio"][value="True"]').prop('checked', false);
            });

            console.log('power user can be selected now!');
            
        }
        
        

    });
    setRemoveAnyDisabled('.removeDisabled');
});