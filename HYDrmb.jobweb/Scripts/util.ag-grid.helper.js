(function (gridHelper, $, uiLb, gridLb) {
  function spaceKeyDispatch(el, timecnt) {
    if (el && el.length) {
      setTimeout(() => {
        el[0].dispatchEvent(
          new KeyboardEvent("keydown", {
            key: " ",
            code: "Space",
            keyCode: 32,
            which: 32,
          })
        );
      }, timecnt);
    }
  }

  function getfiltertype(mtype) {
    if (mtype == "string") return "agTextColumnFilter";

    if (mtype == "number") return "agNumberColumnFilter";

    // no boolean, datetime ()
    return false;
  }

  function createGridColumns(hidefields, columnselector, hidefilterclass, sorterattr, optionsattr, skipclass, fieldidattr, leastwidth) {
    let gridColumns = [];
    if (!columnselector) {
      columnselector = "div.columns span";
    }
    if (!sorterattr) {
      sorterattr = "sorter";
    }
    if (!optionsattr) {
      sorterattr = "options";
    }
    if (!skipclass) {
      skipclass = "skip-col";
    }
    if (!leastwidth) {
      leastwidth = 150;
    }
    if (!fieldidattr) {
      fieldidattr = "id";
    }
    if (!hidefields) {
      hidefields = [];
    }
    if (!hidefilterclass) {
      hidefilterclass = "hide-filter";
    }

    $(columnselector).each(function (i, v) {
      let sorter = $(v).attr(sorterattr);
      let options = $(v).attr(optionsattr);
      var hidefilter = $(v).hasClass(hidefilterclass);
      if (skipclass && $(v).hasClass(skipclass)) {
        return;
      }
      let col = {
        headerName: $(v).text(),
        field: $(v).attr(fieldidattr),
        minWidth: leastwidth,
        filter: getfiltertype(sorter),
        floatingFilter: true,
      };

      gridColumns.push(col);

      if (hidefields.indexOf(col.field) >= 0) {
        col.hide = true;
        col.suppressToolPanel = true;
      }
      if (!col.filter  || hidefilter) {
        col.floatingFilter = false;
        //skipping the filter params setting
      } else {
        // assume col.filter is ready
        if (sorter == "number") {
          col["filterParams"] = {
            filterOptions: ["equals", "lessThan", "greaterThan"],
            suppressAndOrCondition: true,
          };
        } else {
          let extOpts = [];
          if (options && options.length > 0) {
            extOpts = extOpts.concat(options.split("|"));
          }
          col["floatingFilterComponentParams"] = { suppressFilterButton: true };

          col["filterParams"] = {
            filterOptions: ["contains"].concat(extOpts),
            suppressAndOrCondition: true,

            textMatcher: function (textparams) {
              console.log(textparams);

              if (textparams.filter !== "contains") {
                return value == textparams.filter;
              }

              let rg = new RegExp(textparams.filterText, "gi");
              if (rg.test(value)) return true;

              return false;
            },
          };
        }
      }
    });

    return gridColumns;
  }

  function setAutoCompleteOnGridReadyFn(targetselector, sourceattr, clearorginselector, wantedpropattr) {
    var autolist = [];
    if (!targetselector) {
      targetselector = "autocp-";
    }
    if (!sourceattr) {
      sourceattr = "urlis";
    }

    //this propname is for auto complete to locate the specific field in autocomplete , so it cannot be changed up to this moment.
    var propattr = "propname";

    var autoselector = '.ag-header-row div[class*="' + targetselector + '"]';
    var listofautourl = '.ag-header-row div[ref="eFloatingFilterInput"]';
    var filterbodyselector = 'div[ref="eFloatingFilterBody"]';

    if (!clearorginselector) {
      clearorginselector = ".clearcontainer.origin";
    }

    var autocpSelector = "." + targetselector.slice(0, -1);
    //debug
    console.log("auto cp selector ", autocpSelector);

    //grid ready function  (params is from the ag-grid ready event)
    return function (params) {
      // the autoselector is searching the grid floating filter for creating the autocomplete
      // this autocp- selector built-in by customized script from ag-grid
      $(autoselector).each(function (i, el) {
        autolist.push(el);
        var $nextrow = $(el).parent().next();
        var $filterbody = $nextrow.find(filterbodyselector);
        var $input = $($filterbody[i]).find("input");
        //this is used to capture id prefix inside autoclass
        var autoclass = "";
        if ($input && $input.length) {
          el.classList.forEach(function (e, i) {
            if (e.startsWith(targetselector)) {
              autoclass = e.replace(targetselector, "");
              return;
            }
          });

          var $colspan = $('div.columns span[id="' + autoclass + '"]');
          var wantedpropname = autoclass;
          if (wantedpropattr) {
            var foundnewpropname = $colspan.attr(wantedpropattr);
            if (foundnewpropname) {
              wantedpropname = foundnewpropname;
            }
          }

          if ($colspan.attr("options")) {
            $input.attr(sourceattr, $colspan.attr(sourceattr));
            $input.attr(propattr, wantedpropname);
            $input.addClass(targetselector.slice(0, -1));
          }
        }
      });

      uiLb.Core.setupAutoComplete(
        autocpSelector,
        sourceattr,
        null,
        function (el, val) {
          //select handle
          //the below is only helping to understand the filter inside the api
          //var thisapi = params.api;
          //var thisfilter = thisapi.getFilterInstance(el.attr('propname'));
          //thisfilter.applyModel();
          //thisfilter.eventService.dispatchEvent(new Event('change',{bubbles: true}));
          spaceKeyDispatch(el.parent().parent(), 100);
        },
        function (vl, isblur) {
          //blur handle
          //try to clone clear button piece from layout
          //class origin just to avoid misleading for clone version
          //because the clear button piece will attach to the floating container by grid
          var clearClselector = clearorginselector.slice(0, clearorginselector.lastIndexOf("."));
          var originClass = clearorginselector.slice(clearClselector.length + 1);
          if (!isblur) {
            var $clearUI = $(clearClselector, vl.parent());
            if (!$clearUI || $clearUI.length == 0) {
              vl.parent().append($("body " + clearorginselector).clone());
              $clearUI = $(clearClselector, vl.parent());
              $clearUI.removeClass(originClass);
              $clearUI.attr("tabindex", -1);
            }
            $clearUI.removeClass("hidden");
            $clearUI.off("click");
            $clearUI.on("click", function () {
              vl.val("");
              $clearUI.addClass("hidden");
              spaceKeyDispatch(vl.parent().parent(), 100);
            });
          } else {
            var $clearUI = $(clearClselector, vl.parent());
            if (!vl.val()) {
              $clearUI.addClass("hidden");
            }
          }
        }
      );
    };
  }

  function createGrid(
    gridholderInstance,
    initdata,
    //url base for grid data
    sourceurlbase,
    //url part to identify user
    utag,
    //row count selector
    rowcountselector,
    //hide private columns (with grid column but hidden)
    hidefields,
    //skip private columns (i.e. not grid column )
    skipclass,
    //hide filter attr
    hidefilterclass,
    //auto complete source url attr
    sourceattr,
    //autocp selector marked in columns defined in server page
    targetselector,
    sortandfilterfn,
    selectionChangedfn,
    doubleClickedfn,
    //locate auto complete url custom propname
    wantedpropattr,
    //locate the cross mark from layout for cloning that is used for autocomplete field clear fn
    clearorginselector,
    // html element wrapper where columns defined from server page
    columnselector,
    // attr define the data type of corresponding field
    sorterattr,
    // available options provided from server page for choosing in grid filter (although options source url giving same options)
    optionsattr,
    fieldidattr,
    leastwidth
  ) {
    var dataSource = createDataSource(initdata, sourceurlbase, utag, rowcountselector);
    const cols = createGridColumns(hidefields, columnselector,hidefilterclass, sorterattr, optionsattr, skipclass, fieldidattr, leastwidth);
    const colsDefs = [
      {
        checkboxSelection: true,
        colId: "root",
        headerName: "",
        width: 50,
        maxWidth: 50,
        sortable: false,
        resizable: false,
        hide: true,
      },
    ].concat(cols);

    var gridOptions = {
      columnDefs: colsDefs,
      context: {
        sortandfilter: sortandfilterfn,
        getsource: dataSource.getRawData,
      },
      onSelectionChanged: selectionChangedfn,
      onRowDoubleClicked: doubleClickedfn,
      onGridReady: setAutoCompleteOnGridReadyFn(targetselector, sourceattr, clearorginselector, wantedpropattr),
      onFirstDataRendered: (params) => {
        params.api.sizeColumnsToFit();
      },
      defaultColDef: {
        flex: 1,
        sortable: true,
        resizable: true,
        floatingFilter: false,
        filter: "agTextColumnFilter",
        minWidth: 100,
        suppressMenu: true,
      },
      rowBuffer: 0,
      rowSelection: "multiple",
      // tell grid we want virtual row model type
      rowModelType: "infinite",
      // how big each page in our page cache will be, default is 100
      cacheBlockSize: 100,
      // how many extra blank rows to display to the user at the end of the dataset,
      // which sets the vertical scroll and then allows the grid to request viewing more rows of data.
      // default is 1, ie show 1 row.
      cacheOverflowSize: 2,
      // how many server side requests to send at a time. if user is scrolling lots, then the requests
      // are throttled down
      maxConcurrentDatasourceRequests: 1,
      // how many rows to initially show in the grid. having 1 shows a blank row, so it looks like
      // the grid is loading from the users perspective (as we have a spinner in the first col)
      infiniteInitialRowCount: 1000,
      // how many pages to store in cache. default is undefined, which allows an infinite sized cache,
      // pages are never purged. this should be set for large data to stop your browser from getting
      // full of data
      maxBlocksInCache: 10,

      // debug: true,
    };

    let mygrid = new gridLb.Grid(gridholderInstance, gridOptions);
    gridOptions.api.setDatasource(dataSource);
    return mygrid;
  }

  function createDataSource(initdata, sourceurlbase, utag, rowcountselector) {
    if (!rowcountselector) {
      rowcountselector = "#rowcount";
    }
    let data = initdata;
    const dataSource = {
      getRawData: function () {
        return data;
      },
      rowCount: undefined,
      // behave as infinite scroll
      //please note that the filterModel filter needs to self-escape '&' sign
      getRows: (params) => {
        let hit = false;
        if (!params.startRow) {
          let customapi = sourceurlbase + "/" + encodeURI(utag) + "/";
          let fromDate = $("#DateFrom").val() || "01/01/1900";
          let toDate = $("#DateTo").val() || "30/12/9999";
          fromDate = moment(fromDate, "DD/MM/YYYY");
          fromDate = fromDate.format("YYYYMMDD");
          toDate = moment(toDate, "DD/MM/YYYY");
          toDate = toDate.format("YYYYMMDD");

          customapi += fromDate + "/";
          customapi += toDate + "/";

          if (params.filterModel) {
            let search = "";
            let type = "";

            Object.keys(params.filterModel).forEach(function (v, i) {
              if (params.filterModel[v].type == "contains" && params.filterModel[v].filter) {
                search += params.filterModel[v].filter.replace("&", "`3").replace("/", "`8") + "!";
                type += v + "!";
                hit = true;
              } else if (params.filterModel[v].type && params.filterModel[v].filter.trimEnd() == params.filterModel[v].type) {
                search += params.filterModel[v].type.replace("&", "`3").replace("/", "`8") + "!";
                type += v + "!";
                hit = true;
              } else if (params.filterModel[v].filter) {
                search += params.filterModel[v].filter.replace("&", "`3").replace("/", "`8") + "!";
                type += v + "!";
                hit = true;
              }
            });
            if (hit) {
              customapi += encodeURIComponent(search.substring(0, search.length - 1)) + "/" + encodeURIComponent(type.substring(0, type.length - 1));
            }
          }

          if (params.sortModel && params.sortModel.length > 0) {
            if (!hit) {
              customapi += "/sortonly";
            }

            customapi += "/" + params.sortModel[0].colId + "/" + params.sortModel[0].sort;
          }
          //debug
          console.log("try to check custom api for bookings", customapi);
          $.get(customapi, function (r) {
            data = r;
            const rowsasyncPage = data.slice(params.startRow, params.endRow);

            params.context.needWait = false;
            let lastRow = -1;
            if (data.length <= params.endRow) {
              lastRow = data.length;
            } else {
              params.context.needWait = true;
            }
            $(rowcountselector).text(data.length);
            // call the success callback
            params.successCallback(rowsasyncPage, lastRow);
          });
        } else {
          //if (params.sortModel && params.sortModel.length > 0) {
          //    if (params.sortModel[0].colId == "StctNo") {
          //        params.sortModel[0].colId = "FormattedStctNo";
          //    }
          //}
          let filterdata = params.context.sortandfilter(params.sortModel, params.filterModel, data);
          const rowsThisPage = filterdata.slice(params.startRow, params.endRow);

          params.context.needWait = false;
          let lastRow = -1;
          if (filterdata.length <= params.endRow) {
            lastRow = filterdata.length;
          } else {
            params.context.needWait = true;
          }
          // call the success callback
          params.successCallback(rowsThisPage, lastRow);
        }
      },
    };
    return dataSource;
  }

  console.log(`test gridhelper see uiLb & gridLb yet?`, uiLb, gridLb);

  gridHelper.Core = {
    spaceKeyDispatch: spaceKeyDispatch,
    setAutoCompleteOnGridReadyFn: setAutoCompleteOnGridReadyFn,
    createGridColumns: createGridColumns,
    createDataSource: createDataSource,
    createGrid: createGrid,
  };

  return gridHelper;
})((this.gridHelper = this.gridHelper || {}), jQuery, uicontrolLib, agGrid);
