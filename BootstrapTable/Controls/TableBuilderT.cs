using BootstrapTable.Support;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Mvc;

namespace BootstrapTable.Controls
{
    /// <summary>
    /// Build a BootstrapTable control.
    /// </summary>
    internal partial class TableBuilderT<TModel> : TableBuilder, IColumnBuilderT<TModel>
    {
        /// <exclude/>
        public TableBuilderT(string id = null, string url = null, TablePaginationOption sidePagination = TablePaginationOption.none, object htmlAttributes = null)
            : base(id, url, sidePagination, htmlAttributes)
        {
            AddInfo(typeof(TModel).GetSortedProperties()) 
            .ForEach(pair =>
            {
                var tm = typeof(TModel);
                
                DisplayAttribute display = pair.Value;
                HiddenInputAttribute hidden = pair.Key.GetCustomAttribute<HiddenInputAttribute>();

                if (hidden == null || hidden.DisplayValue == true)
                {
                    if (display == null || display.GetAutoGenerateField() == null || display.AutoGenerateField == true)
                    {
                        if (display != null && !string.IsNullOrEmpty(display.GetName()))
                            Column(display.GetName(), pair.Key.Name);
                        else
                            Column(pair.Key.Name.SplitCamelCase(), pair.Key.Name);
                    }
                }
            });
        }

        #region ITableApply
        /// <inheritDoc/>
        new public ITableBuilderT<TModel> ApplyToTable(string name, object value)
        {
            base.ApplyToTable(name, value);
            return this;
        }

        /// <inheritDoc/>
        public new ITableBuilderT<TModel> ApplyToColumns(ColumnOption option)
        {
            base.ApplyToColumns(option);
            return this;
        }

        /// <inheritDoc/>
        new public ITableBuilderT<TModel> Apply(params TableOption[] options)
        {
            base.Apply(options);
            return this;
        }

        /// <inheritDoc/>
        new public ITableBuilderT<TModel> Apply(TableOption option, object value)
        {
            base.Apply(option, value);
            return this;
        }

        /// <inheritDoc/>
        new public ITableBuilderT<TModel> Apply(TableOption option, object[] value)
        {
            base.Apply(option, value);
            return this;
        }

        /// <inheritDoc/>
        new public ITableBuilderT<TModel> Cease(params TableOption[] options)
        {
            base.Cease(options);
            return this;
        }

        /// <inheritDoc/>
        new public ITableBuilderT<TModel> Columns(params string[] columns)
        {
            base.Columns(columns);
            return this;
        }
        #endregion //ITableApply

        #region IColumnApply
        /// <inheritDoc/>
        public new IColumnBuilderT<TModel> Apply(ColumnOption option, object[] value)
        {
            base.Apply(option, value);
            return this;
        }

        /// <inheritDoc/>
        IColumnBuilderT<TModel> IColumnApply<IColumnBuilderT<TModel>>.Apply(ColumnOption option, object value)
        {
            base.Apply(option, value);
            return this;
        }

        /// <inheritDoc/>
        IColumnBuilderT<TModel> IColumnApply<IColumnBuilderT<TModel>>.Apply(params ColumnOption[] options)
        {
            base.Apply(options);
            return this;
        }

        /// <inheritDoc/>
        public new IColumnBuilderT<TModel> Cease(params ColumnOption[] options)
        {
            base.Cease(options);
            return this;
        }
        #endregion //IColumnApply

        #region ITableBuilderT
        /// <inheritDoc/>
        public IColumnBuilderT<TModel> Apply<TProperty>(Expression<Func<TModel, TProperty>> expression, ColumnOption option, object[] value)
        {
            ApplyToColumnT(expression, option.FieldName(), string.Format("[{0}]", string.Join(",", value)));
            return this;
        }

        /// <inheritDoc/>
        public IColumnBuilderT<TModel> Apply<TProperty>(Expression<Func<TModel, TProperty>> expression, ColumnOption option, object value)
        {
            ApplyToColumnT(expression, option.FieldName(), value);
            return this;
        }

        /// <inheritDoc/>
        public IColumnBuilderT<TModel> Apply<TProperty>(Expression<Func<TModel, TProperty>> expression, params ColumnOption[] options)
        {
            options.ForEach(option =>
            {
                ApplyToColumnT(expression, option.FieldName(), option.FieldValue() ?? true.ToStringLower());
            });
            return this;
        }

        /// <inheritDoc/>
        public IColumnBuilderT<TModel> Cease<TProperty>(Expression<Func<TModel, TProperty>> expression, params ColumnOption[] options)
        {
            options.ForEach(option =>
            {
                ApplyToColumnT(expression, option.FieldName(), option.FieldValue() ?? false.ToStringLower());
            });
            return this;
        }

        /// <inheritDoc/>
        public IColumnBuilderT<TModel> Column<TProperty>(Expression<Func<TModel, TProperty>> expression)
        {
            var member = expression.Body as MemberExpression;
            SetColumnByTitle(GetDisplayName((PropertyInfo)member.Member));
            return this;
        }
        #endregion //ITableBuilderT

        /// <exclude/>
        protected IColumnBuilderT<TModel> ApplyToColumnT<TProperty>(Expression<Func<TModel, TProperty>> expression, string name, object value)
        {
            var member = expression.Body as MemberExpression;

            if (!SetColumnByTitle(GetDisplayName((PropertyInfo)member.Member)))
                throw new ArgumentException("Column not found!");
            base.ApplyToColumn(name, value);
            return this;
        }
    }
}
