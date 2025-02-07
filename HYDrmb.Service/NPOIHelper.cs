using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Eval;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using HYDrmb.Abstraction;

namespace HYDrmb.Service
{
    public static class NPOIHelper
    {
        public static bool TestWrite(string filename,string targetfile)
        {
            try
            {
                using(FileStream fs =new FileStream(filename,FileMode.Open,FileAccess.Read))
                using (FileStream ws =new FileStream(targetfile,FileMode.Create,FileAccess.Write))
                
                {                    
                    var workbook = new HSSFWorkbook(fs);
                    ISheet sheet = workbook.GetSheetAt(0);
                    var row = sheet.GetRow(4);
                    var cellr = new NPOI.SS.Util.CellReference($"B2");
                    var cell = row.GetCell(cellr.Col);
                    cell.SetCellValue("test 1");


                    workbook.Write(ws);
                    workbook.Close();

                    return true;

                }
            }
            catch(Exception ex)
            {

            }
            return false;
        }

        public static bool WriteDataTable(string filename,string targetfile,DataTable dt,bool isXSSFWorkbook=false)
        {
            try
            {
                using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                using (FileStream ws = new FileStream(targetfile, FileMode.Create, FileAccess.Write))

                {
                    IWorkbook workbook = isXSSFWorkbook ? (IWorkbook)new XSSFWorkbook(fs) : (IWorkbook)new HSSFWorkbook(fs);
                    ISheet sheet = workbook.GetSheetAt(0);
                    var headerrow = sheet.GetRow(0);
                    if(headerrow==null)
                        headerrow=sheet.CreateRow(0);
                    var nextrowpos = 1;

                    var font = workbook.CreateFont();
                    font.FontHeightInPoints = 11;
                    font.FontName = "Calibri";
                    font.IsBold = true;
                    

                    foreach (DataColumn c in dt.Columns)
                    {
                        var headercell = headerrow.GetCell(dt.Columns.IndexOf(c));
                        if (headercell == null) headercell = headerrow.CreateCell(dt.Columns.IndexOf(c));
                        headercell.SetCellValue(c.Caption);
                        headercell.CellStyle = workbook.CreateCellStyle();
                        headercell.CellStyle.SetFont(font);

                    }
                    
                    foreach(DataRow dtr in dt.Rows)
                    {
                        var newrow = sheet.GetRow(nextrowpos) ?? sheet.CreateRow(nextrowpos);
                        nextrowpos++;

                        foreach(DataColumn c in dt.Columns)
                        {
                            var colindex = dt.Columns.IndexOf(c);
                            var newcell = newrow.GetCell(colindex) ?? newrow.CreateCell(colindex);
                            if( dtr[c] != null && dtr[c] != DBNull.Value)
                            {
                                if (TypeExtensions.BaseType(c.DataType) == typeof(DateTime))
                                {
                                    newcell.SetCellValue(((DateTime)dtr[c]).ToString("dd/MM/yyyy"));
                                }
                                else if (TypeExtensions.BaseType(c.DataType) == typeof(int) )
                                {
                                    newcell.SetCellValue((int)dtr[c]);
                                }
                                else
                                {
                                    newcell.SetCellValue(dtr[c]?.ToString());
                                }

                            }


                        }
                    }

                    
                        //workbook.GetCreationHelper().CreateFormulaEvaluator().EvaluateAll();

                    workbook.Write(ws);
                    workbook.Close();

                    return true;

                }
            }
            catch (Exception ex)
            {

            }
            return false;
        }

        public static bool WriteDataTable(string filename, string targetfile,Dictionary<int,DataTable> dts,bool isXSSFWorkbook=false,bool eval=true)
        {
            try
            {
                using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                using (FileStream ws = new FileStream(targetfile, FileMode.Create, FileAccess.Write))

                {
                    IWorkbook workbook =isXSSFWorkbook ? (IWorkbook)new XSSFWorkbook(fs):  (IWorkbook)new HSSFWorkbook(fs);
                    ISheet sheet = workbook.GetSheetAt(0);
                    foreach(var k in dts.Keys)
                    {
                        int rx = k;
                        foreach(DataRow r in dts[k].Rows.OfType<DataRow>())
                        {
                            //int cx = 0;
                            rx = dts[k].Rows.IndexOf(r) + k;
                            foreach(DataColumn c in dts[k].Columns.OfType<DataColumn>())
                            {
                                var cx = dts[k].Columns.IndexOf(c);
                                var row = sheet.GetRow(rx);


                                
                                if (r[c] != DBNull.Value)
                                {

                                    if (row == null)
                                    {
                                        row = sheet.CreateRow(rx);
                                    }



                                    string cellstr = c.DataType == typeof(string) ? (r.Field<string>(c) ?? "") : null;
                                    var isDecimal = c.DataType == typeof(decimal);
                                    decimal celldec = cellstr == null && isDecimal ? r.Field<decimal>(c) : -1;
                                    int cellint = (cellstr == null && !isDecimal) ? r.Field<int>(c) : -1;

                                    var cell = row.GetCell(cx);
                                    if (cell == null)
                                        cell = row.CreateCell(cx);

                                    //Please don't use continue statement to skip line
                                    if (cellstr != null)
                                        cell.SetCellValue(cellstr);
                                    else
                                    {
                                        if (isDecimal)
                                        {
                                            
                                            cell.SetCellValue(Decimal.ToDouble(celldec));
                                        }
                                        else cell.SetCellValue(cellint);
                                    }
                                }



                                
                            }
                            
                            
                        }
                    }
                    if (eval)
                        workbook.GetCreationHelper().CreateFormulaEvaluator().EvaluateAll();
                    
                    workbook.Write(ws);
                    workbook.Close();

                    return true;

                }
            }
            catch (Exception ex)
            {

            }
            return false;
        }
    }
}
