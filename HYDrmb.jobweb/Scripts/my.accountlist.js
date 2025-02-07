function setupVIP(vipselector, normalselector, formselector) {
  $(vipselector).off("click");
  $(normalselector).off("click");
  $(vipselector).on("click", function (e) {
    var $el = $(e.target);
    var url = $el.attr("urlis");
    $.get(url, function (r) {
      if (!r || r.status == "success") {
        $(formselector).submit();
      } else {
        BootstrapDialog.show({
          title: "VIP Enabled Failure",
          message: r.msg,
          type: BootstrapDialog.TYPE_DANGER,
          buttons: [
            {
              id: "closebtn",
              label: "Close",
              action: function (dialog) {
                dialog.close();
              },
            },
          ],
        });
      }
    });
  });
  $(normalselector).on("click", function (e) {
    var $el = $(e.target);
    var url = $el.attr("urlis");
    $.get(url, function (r) {
      if (!r || r.status == "success") {
        $(formselector).submit();
      } else {
        alertLib.Core.alert('VIP Disabled Failure', r.msg, 'Close', { mode: 'danger' });

      }
    });
  });
}

function setupDelete(delclass, formselector) {
  $(delclass).off("click");
  $(delclass).on("click", function (e) {
    var $el = $(e.target);
    var url = $el.attr("urlis");
    alertLib.Core.askYesNo("Disable Account?", "Are you sure to disable the selected account?", "No").done(function (r) {
      if (r.yes) {
        $.get(url, function (r) {
          if (!r || r.status == "success") {
            alertLib.Core.alert("Selected Account Disabled!", "The action completed!", "OK");
            $(formselector).submit();
          } else {
            alertLib.Core.alert("Account Disabling Error!", r.message, "OK");
          }
        });
      }
    });
  });
}
function setupclick(btnimport, filewrap, shwallclass, resetclass, formselector, filterselector) {
  var hanldes = uicontrolLib.Core.setUploadButton(filewrap, function (files) {
    var posturl = $("form.form-file").attr("action");
    var ajaxSubmit = $.ajaxQueue({
      type: "POST",
      url: posturl,
      processData: false,
      contentType: false,
      data: new FormData($("form.form-file")[0]),
    });
    var needClear = $.Deferred();
    ajaxSubmit.jload().done(function (data) {
      needClear.resolve(data);

      alertLib.Core.alert("File upload!", data.count + " User Records imported ", "OK", data);
    });
    return needClear;
  });
  $(btnimport).click(function (e) {
    e.preventDefault();
    e.stopPropagation();
    hanldes.importhandle();
  });
  $(shwallclass).change(function (e) {
    setTimeout(() => {
      $(formselector).submit();
    }, 500);
  });
  let $userinput = $(filterselector);
  $(resetclass).click(function (e) {
    e.preventDefault();
    e.stopPropagation();
    $userinput.val("");
    $(formselector).submit();
  });

  $userinput.focus();
  if ($userinput.val()) {
    $userinput.select();
  }
}

$(document).ready(function () {
  setupVIP(".setvip-btn", ".setnormal-btn", "#genUserList");
  setupDelete(".delete-btn", '#genUserList');
  themeLib.Core.setupTheme(null);
  uicontrolLib.Core.setupUnderline();

  setupclick(
    "a.import-btn",
    ".container-file",
    'input[type="checkbox"][id*="ShowAll"]',
    "button.reset-btn",
    "#genUserList",
    'input[name*="AskUserName"]'
  );
  uicontrolLib.Core.setpageLink(".page-link", ".page-val", "linkto", "th[sort]", ".sort-val");
});
