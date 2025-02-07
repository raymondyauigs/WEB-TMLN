(function (uicontrolLib, $, $imask) {
  function isElement(element) {
    return element instanceof Element || element instanceof HTMLDocument;
  }
  function hitEscapeKey() {
    // Browser bug: can't use keyboard events, so we do it this way.
    // See https://tinyurl.com/mholrhv
    window.dispatchEvent(
      new KeyboardEvent("keypress", {
        altKey: false,
        code: "Escape",
        ctrlKey: false,
        isComposing: false,
        key: "Escape",
        location: 0,
        metaKey: false,
        repeat: false,
        shiftKey: false,
        which: 27,
        charCode: 0,
        keyCode: 27,
      })
    );
  }

  function hideshowSection(choiceselector, ishide, parentselector) {
    var sectionlist = [];
    if (!parentselector) {
      parentselector = 'div[class^="mdc-layout-grid"]';
    }
    $(choiceselector).each(function (i, v) {
      if ($(v).is("textarea") || $(v).is("input")) {
        $(v).prop("required", !ishide);

        let vparent = $(v).parent();
        let max = 4;
        do {
          vparent = $(vparent).parent();
          if ($(vparent).is(parentselector)) {
            ishide && $(vparent).hide(), !ishide && $(vparent).show(), !ishide && $(vparent).siblings("label").show();
            if (vparent) {
              var ele = isElement(vparent) ? vparent : vparent[0];
              sectionlist.push(ele);
            }

            break;
          }
          max--;
        } while (max >= 0);
      } else {
        ishide && $(v).hide(), !ishide && $(v).show();
        $(v).find("select").prop("required", !ishide);

        if ($(v)) {
          var ele = isElement(v) ? v : $(v)[0];
          sectionlist.push(ele);
        }
      }
    });

    return sectionlist;
  }

  function setupSectionShow(selname, hidehandler, showhandler, negatesuffix, parentselector) {
    $('select[name="' + selname + '"],' + 'input[type="radio"][name="' + selname + '"]').off("change.section");
    $('select[name="' + selname + '"],' + 'input[type="radio"][name="' + selname + '"]').on("change.section", function (e) {
      let $sel = $(e.target);
      const optval = $sel.val();
      const hasNotOption = true;

      if (!negatesuffix) {
        negatesuffix = "";
        hasNotOption = false;
      }

      if ($sel.is("select")) {
        $('option:not([value="' + optval + '"])', $sel).each(function (i, el) {
          var $nopt = $(el);
          const noptval = $nopt.val();
          const showselector = "." + selname + "-" + noptval + negatesuffix;
          if (noptval) {
            //hasNotOption normally is false => show action
            var showlist = hideshowSection(showselector, hasNotOption, parentselector);

            if (showhandler && !hasNotOption) {
              showhandler(showlist, $sel, optval);
            }
            if (hidehandler && hasNotOption) {
              hidehandler(hidelist, $sel, optval);
            }
          }
        });

        const hideselector = "." + selname + "-" + optval + negatesuffix;
        var hidelist = hideshowSection(hideselector, !hasNotOption, parentselector);

        if (hidehandler && !hasNotOption) {
          hidehandler(hidelist, $sel, optval);
        }
        if (showhandler && hasNotOption) {
          showhandler(hidelist, $sel, optval);
        }
      } else if ($sel.is(":checked")) {
        $('input[type="radio"][name="' + $sel.attr("name") + '"]:not([value="' + optval + '"])').each(function (i, el) {
          var $nopt = $(el);
          const noptval = $nopt.val();
          const showselector = "." + selname + "-" + noptval + negatesuffix;
          if (noptval) {
            var showlist = hideshowSection(showselector, hasNotOption, parentselector);

            if (showhandler && !hasNotOption) {
              showhandler(showlist, $sel, optval);
            }
            if (hidehandler && hasNotOption) {
              hidehandler(showlist, $sel, optval);
            }
          }
        });

        const hideselector = "." + selname + "-" + optval + negatesuffix;
        var hidelist = hideshowSection(hideselector, !hasNotOption, parentselector);

        if (hidehandler && !hasNotOption) {
          hidehandler(hidelist, $sel, optval);
        }
        if (showhandler && hasNotOption) {
          showhandler(hidelist, $sel, optval);
        }
      }
    });
    $('select[name="' + selname + '"],' + 'input[type="radio"][name="' + selname + '"]').trigger("change.section");
  }

  function removeAnyDisabled(classDisabled) {
    $("input,select", classDisabled).prop("disabled", false);
    $(classDisabled).prop("disabled", false);
    $(".mdc-form-field input:not(:visible)")
      .parents(".mdc-form-field")
      .parent()
      .each(function (i, e) {
        $("*", $(e)).show();
        $(e).show();
      });
  }
  function setupMask(inputselector, regex, restregex) {
    var masked = [];
    if ($(inputselector).length > 0) {
      $(inputselector).each(function (i, e) {
        var ownmask = $imask(e, {
          prepareChar: function (str) {
            return str.toUpperCase();
          },

          mask: function (value) {
            return regex.test(value);
          },
        });

        masked.push(ownmask);
      });
    }
    return masked;
  }

  function setupDrop(wrap, inputselector, clearselector, submitselector, onChange) {
    const dropContainer = $(wrap)[0];
    const fileInput = $(inputselector)[0];
    var $file = $(fileInput);

    dropContainer.addEventListener(
      "dragover",
      (e) => {
        // prevent default to allow drop
        e.preventDefault();
      },
      false
    );

    dropContainer.addEventListener("dragenter", () => {
      dropContainer.classList.add("drag-active");
    });

    dropContainer.addEventListener("dragleave", () => {
      dropContainer.classList.remove("drag-active");
    });

    dropContainer.addEventListener("drop", (e) => {
      e.preventDefault();
      dropContainer.classList.remove("drag-active");
      fileInput.files = e.dataTransfer.files;
      $file.trigger("change");
    });
    $(clearselector).off("click.cleardrop");
    $(clearselector).on("click.cleardrop", function (e) {
      e.preventDefault();
      e.stopPropagation();

      $file[0].value = "";
      if (!/safari/i.test(navigator.userAgent)) {
        $file[0].type = "";
        $file[0].type = "file";
      }
      $(submitselector)[0].disabled = true;
      $file.trigger("change");
    });

    $file.on("change", function (e) {
      var files = [];
      if (typeof e.currentTarget.files) {
        if (e.currentTarget.files.length > 0) {
          $(submitselector)[0].disabled = false;
        }
        for (var i = 0; i < e.currentTarget.files.length; i++) {
          files.push(e.currentTarget.files[i].name.split("\\/").pop());
        }
      } else {
        files.push($(e.currentTarget).val().split("\\/").pop());
      }

      if (onChange) {
        onChange(files);
      }
    });
  }

  function setUploadButton(wrap, filechangelistener) {
    var $group = $(wrap + " .input-group");
    var $file = $group.find('input[type="file"]');
    var $browse = $group.find('[data-action="browse"]');
    var $fileDisplay = $group.find('[data-action="display"]');
    var $reset = $group.find('[data-action="reset"]');

    var resetHandler = function (e) {
      if ($file.length === 0) {
        return;
      }

      $file[0].value = "";
      if (!/safari/i.test(navigator.userAgent)) {
        $file[0].type = "";
        $file[0].type = "file";
      }
      $('input[type="submit"]')[0].disabled = true;
      $file.trigger("change");
    };

    var browseHandler = function (e) {
      //If you select file A and before submitting you edit file A and reselect it it will not get the latest version, that is why we  might need to reset.
      //resetHandler(e);
      $file.trigger("click");
    };

    $browse.on("click", function (e) {
      //if (event.which != 1) {
      //	return;
      //}
      browseHandler();
    });
    $fileDisplay.on("click", function (e) {
      //if (event.which != 1) {
      //	return;
      //}
      browseHandler();
    });
    $reset.on("click", function (e) {
      //if (event.which != 1) {
      //	return;
      //}

      resetHandler();
    });

    $file.on("change", function (e) {
      var files = [];
      if (typeof e.currentTarget.files) {
        if (e.currentTarget.files.length > 0) {
          $('input[type="submit"]')[0].disabled = false;
        }
        for (var i = 0; i < e.currentTarget.files.length; i++) {
          files.push(e.currentTarget.files[i].name.split("\\/").pop());
        }
      } else {
        files.push($(e.currentTarget).val().split("\\/").pop());
      }
      $fileDisplay.val(files.join("; "));
      if (filechangelistener && files.length > 0) {
        var needClear = filechangelistener(files);
        needClear.promise().always(function (ret) {
          resetHandler();
        });
      }
    });

    return { importhandle: browseHandler };
  }

  function setActive(el, toActive, activeClass, fillClass) {
    const formField = el.parentNode.parentNode;
    activeClass = activeClass || "form-field--is-active";
    fillClass = fillClass || "form-field--is-filled";
    if (toActive) {
      formField.classList.add(activeClass);
    } else {
      formField.classList.remove(activeClass);
      el.value === "" ? formField.classList.remove(fillClass) : formField.classList.add(fillClass);
    }
  }

  function htmlEncode(value) {
    //create a in-memory div, set it's inner text(which jQuery automatically encodes)
    //then grab the encoded contents back out. The div never exists on the page.
    return $("<div/>").text(value).html();
  }

  function htmlDecode(value) {
    return $("<div/>").html(value).text();
  }

  function saveForm(formclass) {
    if ($(formclass).is("form")) {
      $("body").data(formclass, $(formclass).serialize());
      return true;
    }
    return false;
  }
  function checkFormSaved(formclass) {
    if ($(formclass).is("form")) {
      var savedvalue = $("body").data(formclass);
      if (savedvalue && savedvalue == $(formclass).serialize()) {
        return true;
      }
      return false;
    }
    return true;
  }

  function getFormatString(format) {
    var args = Array.prototype.slice.call(arguments, 1);
    return format.replace(/{(\d+)}/g, function (match, number) {
      return typeof args[number] != "undefined" ? args[number] : match;
    });
  }

  function appendToQueryString(param, val, baseonly) {
    var queryString = window.location.search.replace("?", "");
    var parameterListRaw = queryString == "" ? [] : queryString.split("&");
    var parameterList = {};
    for (var i = 0; i < parameterListRaw.length; i++) {
      var parameter = parameterListRaw[i].split("=");
      parameterList[parameter[0]] = parameter[1];
    }
    parameterList[param] = val;

    var newQueryString = "?";
    for (var item in parameterList) {
      if (parameterList.hasOwnProperty(item)) {
        newQueryString += item + "=" + parameterList[item] + "&";
      }
    }
    newQueryString = newQueryString.replace(/&$/, "");

    if (baseonly) {
      return location.pathname + newQueryString;
    }

    return location.origin + location.pathname + newQueryString;
  }

  function setpageLink(linktag, pagetag, linkattrb, sortselector, sorttag, formselector) {
    if (sortselector) {
      var cursort = $(sorttag).val();
      if (cursort) {
        var curorder = cursort.endsWith("-") ? "desc" : "asc";
        $("table " + sortselector + ".sort-" + cursort.substring(0, cursort.length - 1)).attr("sort", curorder);
      }

      $("table " + sortselector).click(function (e) {
        e.preventDefault();
        e.stopPropagation();
        var thtag = $(e.target) && $(e.target)[0];
        var sortdir = $(thtag).attr("sort");
        sortdir = sortdir == "" ? "asc" : sortdir == "desc" ? "" : "desc";
        $(thtag).attr("sort", sortdir);
        sortdir = sortdir == "" ? "" : sortdir == "asc" ? "+" : "-";

        if (thtag) {
          var sortclass = "";
          thtag.classList.forEach(function (cl) {
            if (cl && cl.startsWith("sort-")) {
              sortclass = cl;
              var $sort = sorttag && $(sorttag);
              if ($sort && sortdir) {
                $sort.val(cl.substring(5) + sortdir);
              } else {
                $sort && $sort.val("");
              }
            }
          });
        }
        if (sortclass) {
          $("table " + sortselector + ":not(." + sortclass + ")").attr("sort", "");
          if (formselector) {
            $(formselector).submit();
          } else {
            $("form").submit();
          }
        }
      });
    }

    $(linktag).click(function (e) {
      var $link = $(e.target);
      if ($link.is("i")) {
        $link = $link.parent("a");
      } else if(!$link.is("a")) {
        $link = $link.parents("a");
      }
      
      e.preventDefault();
      e.stopPropagation();

      if ($link.hasClass("noclick")) return;

      var linkto = $link.attr(linkattrb);

      if (linkto) {
        $(pagetag).val(linkto);
      } else {
        var pageval = $link.text();
        $(pagetag).val(pageval);
      }
      $("form").submit();
    });
  }
  function setupUnderline(inputClass, areaClass) {
    inputClass = inputClass || ".form-field__input";
    areaClass = areaClass || ".form-field__textarea";

    [].forEach.call(document.querySelectorAll(inputClass + "," + areaClass), function (el) {
      el.onblur = function () {
        setActive(el, false);
      };
      el.onfocus = function () {
        setActive(el, true);
      };
    });
  }

  function escapeHtml(unsafe) {
    return unsafe.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;").replace(/'/g, "&#039;");
  }

  function isAdmin() {
    return _isAdmin;
  }

  function getSleep(timeup) {
    return new Promise((resolve) => setTimeout(resolve, timeup));
  }
  function setupAutoComplete(selector, urltag, withintag, selecthandler, clearhandler, toupper) {
    var withinHtml = $(document);
    if (withintag) {
      withinHtml = $(withintag);
    }

    $(selector, withinHtml).each(function (i, e) {
      var apiGet = $(e).attr(urltag);
      var $el = $(e);
      var detecthandler = selecthandler;
      var canwrap = !!clearhandler;

      $(e)
        .autoComplete({
          minChars: 0,
          cache: false,
          upper: !!toupper,
          source: function (searchValue, response) {
            searchValue = $.trim(searchValue);
            var params = { subtext: searchValue };
            var prop = $el.attr("propname");
            var exclude = $el.attr("exclude");
            if (prop) {
              params["propname"] = prop;
              if (exclude) {
                params["exclude"] = exclude;
              }
            }

            var ajaxSubmit = $.ajaxQueue({
              type: "GET",
              url: apiGet,
              data: params,
            });
            ajaxSubmit.jload().done(function (data) {
              response(data);
            });
          },
          onSelect: function (el, search, item) {
            if (item) {
              $el.val(item.data("lang"));
              if (detecthandler) {
                if (item.data("key")) {
                  detecthandler($el, item.data("lang"), item.data("key"));
                } else {
                  detecthandler($el, item.data("lang"));
                }
              }
            }
          },
          renderItem: function (item, search) {
            var rg = /[-/\\^$*+?.()|[\]{}]/g;
            var fkrg = new RegExp("{", "gi");
            var bkrg = new RegExp("}", "gi");
            search = search.replace(rg, "\\$&");

            if (!item || !apiGet) {
              return $("<div></div>").html();
            }
            if (apiGet) {
              var re = new RegExp("(" + search.split(" ").join("|") + ")", "gi");
              var canwrapclass = !!canwrap ? " wrapitem " : "";
              var $renditem = $(
                '<div class="autocomplete-suggestion ' +
                  canwrapclass +
                  '" data-lang="' +
                  item.Value +
                  '" data-val="' +
                  search +
                  '" data-key="' +
                  item.Key +
                  '"> ' +
                  htmlEncode(item.Value.replace(re, "{$1}")).replace(fkrg, "<b>").replace(bkrg, "</b>") +
                  "</div>"
              );

              return $renditem.wrap("div").parent().html();
            }
          },
        })
        .bind("click", function () {
          $(this).trigger("focus");
        })
        .bind("focus", function () {
          if (canwrap) {
            clearhandler($(this));
          }

          if (!$(this).val()) {
            $(this).keydown();
          }
        })
        .bind("blur", function () {
          if (canwrap) {
            clearhandler($(this), true);
          }
        });
    });
  }
  uicontrolLib.Core = {
    setupAutoComplete: setupAutoComplete,
    getFormatString: getFormatString,
    setpageLink: setpageLink,
    setupUnderline: setupUnderline,
    appendToQueryString: appendToQueryString,
    isAdmin: isAdmin,
    saveForm: saveForm,
    checkFormSaved: checkFormSaved,
    getSleep: getSleep,
    setUploadButton: setUploadButton,
    setupDrop: setupDrop,
    setupMask: setupMask,
    setupSectionShow: setupSectionShow,
    removeAnyDisabled: removeAnyDisabled,
    hitEscapeKey: hitEscapeKey,
  };
})((this.uicontrolLib = this.uicontrolLib || {}), jQuery, IMask);
