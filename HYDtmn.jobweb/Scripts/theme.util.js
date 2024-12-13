(function (themeLib, $) {
  function setupText(wrapper) {
    if (!wrapper) {
      wrapper = "";
    }

    const textFields = document.querySelectorAll(wrapper + " .mdc-text-field");
    for (const textField of textFields) {
      mdc.textField.MDCTextField.attachTo(textField);
      $(".mdc-text-field--eye", $(textField)).off("click");
      $(".mdc-text-field--eye", $(textField)).on("click", function (e) {
        console.log(e.target);
        var eyept = $(e.target).parents(".mdc-text-field--eye");
        if ($(".mdc-text-field--eyeopen", eyept).is(":visible")) {
          $(".mdc-text-field--eyeopen", eyept).hide();
          $(".mdc-text-field--eyeclose", eyept).show();
          $(eyept).parents(".mdc-text-field").find("input").attr("type", "text");
        } else {
          $(".mdc-text-field--eyeopen", eyept).show();
          $(".mdc-text-field--eyeclose", eyept).hide();
          $(eyept).parents(".mdc-text-field").find("input").attr("type", "password");
        }
      });
      $(".mdc-text-field--eyeclose", $(textField)).hide();
    }
  }

  function setupRadio(wrapper) {
    if (!wrapper) {
      wrapper = "";
    }
    var radios = document.querySelectorAll(wrapper + " .mdc-radio");
    for (var i = 0, radio; (radio = radios[i]); i++) {
      new mdc.radio.MDCRadio(radio);
    }
  }

  function setupCheckbox(wrapper) {
    if (!wrapper) {
      wrapper = "";
    }

    var checks = document.querySelectorAll(wrapper + " .mdc-checkbox");
    for (var i = 0, check; (check = checks[i]); i++) {
      new mdc.checkbox.MDCCheckbox(check);
      var $inputtag = $(check).find("input");
      $inputtag.off("change");
      $inputtag.on("change", function (e) {
        var hasit = $(e.target).is("input");
        if (hasit) {
          var hasselected = $(e.target).parent("div").hasClass("mdc-checkbox--selected");
          var storetag = $(e.target).next();
          if (storetag.is('input[type="hidden"]')) {
            storetag.val(hasselected);
            storetag.trigger("change");
          }
        }
      });
    }
  }

  function setupButton(wrapper) {
    if (!wrapper) {
      wrapper = "";
    }

    var buttons = document.querySelectorAll(wrapper + " .mdc-button");
    for (var i = 0, btn; (btn = buttons[i]); i++) {
      mdc.ripple.MDCRipple.attachTo(btn);
    }
  }
  function setupSelect(wrapper, labelclass) {
    if (!wrapper) {
      wrapper = "";
    }
    //$(wrapper+ ' select.'+labelclass).off('change.bfarecord');
    //$(wrapper+' select.'+labelclass).on('change.bfarecord',function(e){
    //    let $select = $(e.target);
    //    let hasvalue = $select.find(':selected') && $select.find(':selected').length >0;
    //    //if(hasvalue)
    //    //{
    //    //    $select.prev('span').removeClass('hidden');
    //    //}

    //});
    $(wrapper + " select." + labelclass).trigger("change.bkdbook");

      $(wrapper + " select." + labelclass).off("keydown.bkdbook");
      $(wrapper + " select." + labelclass).on("keydown.bkdbook", function (e) {
      if (e.keyCode == 46 || e.key == "Delete") {
        let $select = $(e.target);
        $select.val("");
        $select.find(":selected").removeAttr("selected");
        $select.trigger("change");
        //$select.prev('span').addClass('hidden');
      }
    });
  }

  function setupTheme(wrapper) {
    setupText(wrapper);
    setupRadio(wrapper);
    setupButton(wrapper);
    setupSelect(wrapper, "theme-select");
    setupCheckbox(wrapper);
  }
  function disabledOthers(removeDisabledclass, onlytagged) {
    if (!onlytagged) {
      onlytagged = "";
    }
    if (!removeDisabledclass) {
      removeDisabledclass = "removeDisabled";
    }

    if (removeDisabledclass) {
      $(onlytagged + " .mdc-radio")
        .find('input[type="radio"]')
        .addClass(removeDisabledclass);
      $(onlytagged + " .mdc-text-field")
        .find('input[type="text"],textarea')
        .addClass(removeDisabledclass);
      $(onlytagged + ' div[include*="form-input-select()"]')
        .find("select")
        .addClass(removeDisabledclass);
    }

    $(onlytagged + " .mdc-radio")
      .find('input[type="radio"]')
      .prop("disabled", true);
    $(onlytagged + " .mdc-text-field")
      .find('input[type="text"],textarea')
      .prop("disabled", true);
    $(onlytagged + ' div[include*="form-input-select()"]')
      .find("select")
      .prop("disabled", true);
  }
  function undisabledOthers(removeDisabledclass) {
    if (!removeDisabledclass) {
        removeDisabledclass = ".removeDisabled";
    }
    $(".mdc-radio").find('input[type="radio"]'+removeDisabledclass).prop("disabled", true);
    $(".mdc-text-field").find('input[type="text"]'+removeDisabledclass+',textarea'+removeDisabledclass).prop("disabled", true);
    $('div[include*="form-input-select()"]').find("select"+removeDisabledclass).prop("disabled", true);
  }

  themeLib.Core = {
    setupTheme: setupTheme,
    setupSelect: setupSelect,
    disabledOthers: disabledOthers,
    undisabledOthers: undisabledOthers,
  };
})((this.themeLib = this.themeLib || {}), jQuery);
