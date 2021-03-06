﻿using OfficeOpenXml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ExportXlsx.Sources
{
    public class TableReader
    {
        // xlsx文件
        public string path;

        // sheet名称
        public string sheetName;

        // 表面
        public string tableName;

        // 数据结构
        public DataStruct dataStruct = new DataStruct();

        public Dictionary<int, DataField> fieldDictByIndex = new Dictionary<int, DataField>();

        // 数据列表
        public List<Dictionary<string, string>> dataList = new List<Dictionary<string, string>>();


        public void Load()
        {
            if(string.IsNullOrEmpty(tableName))
            {
                tableName = Path.GetFileNameWithoutExtension(path).FirstUpper();
            }
            dataStruct.name = tableName;

            var xlsx = new FileInfo(path);
            using (var package = new ExcelPackage(xlsx))
            {

                ExcelWorksheet ws = null;
                if (package.Workbook.Worksheets.Count > 0)
                {
                    IEnumerator enumerator = package.Workbook.Worksheets.GetEnumerator();
                    while (enumerator.MoveNext() && ws == null)
                    {
                        if(string.IsNullOrEmpty(sheetName))
                        {
                            ws = (ExcelWorksheet)enumerator.Current;
                        }
                        else
                        {
                            if (((ExcelWorksheet)enumerator.Current).Name == sheetName)
                            {
                                ws = (ExcelWorksheet)enumerator.Current;
                            }
                        }
                    }
                }

                if(ws == null)
                {
                    Log.Error($"没有找到sheet path:{path}, sheetName:{sheetName}");
                    return;
                }

                if(ws.Cells.Rows < 3)
                {
                    Log.Error($" path:{path}, sheetName:{sheetName}， rows:{ws.Cells.Rows}, 行数小于3行， 必须要有(type, cn, field)");
                    return;
                }

                int columnNum = 0;
                for(int i = 1; i < ws.Cells.Columns; i ++ )
                {

                    if (ws.Cells[1, i].Value == null)
                        break;

                    if (ws.GetValue(2, i) == null)
                    {
                        Log.Error($" path:{path}, sheetName:{sheetName}， 是空单元格 2行{i}列  ");
                        continue;
                    }

                    if (ws.GetValue(3, i) == null)
                    {
                        Log.Error($" path:{path}, sheetName:{sheetName}， 是空单元格 3行{i}列  ");
                        continue;
                    }


                    string type = ws.GetValue(Setting.Options.xlsxHeadTypeLine, i).ToString().Trim();
                    string cn = ws.GetValue(Setting.Options.xlsxHeadCnLine, i).ToString().Trim();
                    string en = ws.GetValue(Setting.Options.xlsxHeadFieldLine, i).ToString().Trim();

                    if (string.IsNullOrEmpty(type))
                    {
                        Log.Error($" path:{path}, sheetName:{sheetName}， 是空单元格 type行{i}列 {type} {cn} {en} ");
                        continue;
                    }

                    if (string.IsNullOrEmpty(en))
                    {
                        Log.Error($" path:{path}, sheetName:{sheetName}， 是空单元格 field{i}列 {type} {cn} {en} ");
                        continue;
                    }

                    DataField field = new DataField();
                    field.typeName = type;
                    field.cn = cn;
                    field.field = en;
                    field.index = i;
                    dataStruct.fields.Add(field);
                    fieldDictByIndex.Add(i, field);
                    columnNum = i;
                }

                for(int r = 4; r < ws.Cells.Rows; r ++)
                {

                    if (ws.Cells[r, 1].Value == null)
                        break;

                    Dictionary<string, string> rowData = new Dictionary<string, string>();
                    for (int c = 1; c <= columnNum; c ++)
                    {
                        string value = string.Empty;

                        if (ws.Cells[r, c].Value != null)
                        {
                            value = ws.GetValue(r, c).ToString().Trim();
                        }

                        if (fieldDictByIndex.ContainsKey(c))
                        {
                            rowData.Add(fieldDictByIndex[c].field, value);
                        }
                    }

                    if(rowData.Count > 0)
                    {
                        dataList.Add(rowData);
                    }

                }



            }
        }
    }
}
