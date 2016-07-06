﻿// The MIT License (MIT)
//
// Copyright (c) 2016 H.J. Lebbink
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using AsmDude.Tools;
using AsmTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace AsmDude.SignatureHelp {
    //the data is retrieved from http://www.nasm.us/doc/nasmdocb.html

    public enum OperandTypeEnum : byte {
        UNKNOWN,

        MEM, MEM8, MEM16, MEM32, MEM64,
        REG8, REG16, REG32, REG64,
        REG_AL, REG_AX, REG_EAX, REG_RAX,
        REG_CL, REG_CX, REG_ECX, REG_RCX,
        REG_DX,
        REG_CS, REG_DS, REG_ES, REG_SS, REG_FS, REG_GS,

        IMM, IMM8, IMM16, IMM32, IMM64,
        imm_imm, imm16_imm, imm_imm16, imm32_imm, imm_imm32,


        RM8, RM16, RM32, RM64,
        sbyteword, sbytedword,
        sbyteword16, sbytedword16,
        sbytedword32,
        sbytedword64,
        udword, sdword,

        near, far, short_ENUM,
        unity,

        mask, z,
        xmmreg, ymmreg, zmmreg,
        xmmreg_star, ymmreg_star, zmmreg_star,
        xmmrm128, ymmrm256, zmmrm512,
        b32, b64,

        reg_sreg, mem_offs, reg_creg, reg_dreg, reg_treg,
        reg32na
    }

    public class SignatureElement {
        public readonly Mnemonic mnemonic;
        public readonly IList<IList<OperandTypeEnum>> operands;

        public SignatureElement(Mnemonic mnem) {
            this.mnemonic = mnem;
            this.operands = new List<IList<OperandTypeEnum>>();
        }

        public bool isAllowed(Operand op, int operandIndex) {
            if (op == null) { return true; }
            if (operandIndex >= this.operands.Count) {
                return false;
            }
            foreach (OperandTypeEnum operandType in this.operands[operandIndex]) {
                if (this.isAllowed_private(op, operandType)) {
                    return true;
                }
            }
            return false;
        }

        private bool isAllowed_private(Operand op, OperandTypeEnum operandType) {

            switch (operandType) {
                case OperandTypeEnum.UNKNOWN: return true;
                case OperandTypeEnum.MEM: return op.isMem;
                case OperandTypeEnum.MEM8: return (op.isMem && op.nBits == 8);
                case OperandTypeEnum.MEM16: return (op.isMem && op.nBits == 16);
                case OperandTypeEnum.MEM32: return (op.isMem && op.nBits == 32);
                case OperandTypeEnum.MEM64: return (op.isMem && op.nBits == 64);
                case OperandTypeEnum.REG8: return (op.isReg && op.nBits == 8);
                case OperandTypeEnum.REG16: return (op.isReg && op.nBits == 16);
                case OperandTypeEnum.REG32: return (op.isReg && op.nBits == 32);
                case OperandTypeEnum.REG64: return (op.isReg && op.nBits == 64);
                case OperandTypeEnum.REG_AL: return (op.isReg && op.rn == Rn.AL);
                case OperandTypeEnum.REG_AX: return (op.isReg && op.rn == Rn.AX);
                case OperandTypeEnum.REG_EAX: return (op.isReg && op.rn == Rn.EAX);
                case OperandTypeEnum.REG_RAX: return (op.isReg && op.rn == Rn.RAX);
                case OperandTypeEnum.REG_CL: return (op.isReg && op.rn == Rn.CL);
                case OperandTypeEnum.REG_CX: return (op.isReg && op.rn == Rn.CX);
                case OperandTypeEnum.REG_ECX: return (op.isReg && op.rn == Rn.ECX);
                case OperandTypeEnum.REG_RCX: return (op.isReg && op.rn == Rn.RCX);
                case OperandTypeEnum.REG_DX:return (op.isReg && op.rn == Rn.DX);

                case OperandTypeEnum.REG_CS: return (op.isReg && op.rn == Rn.CS);
                case OperandTypeEnum.REG_DS: return (op.isReg && op.rn == Rn.DS);
                case OperandTypeEnum.REG_ES: return (op.isReg && op.rn == Rn.ES);
                case OperandTypeEnum.REG_SS: return (op.isReg && op.rn == Rn.SS);
                case OperandTypeEnum.REG_FS: return (op.isReg && op.rn == Rn.FS);
                case OperandTypeEnum.REG_GS: return (op.isReg && op.rn == Rn.GS);

                case OperandTypeEnum.IMM: return op.isImm;
                case OperandTypeEnum.IMM8: return (op.isImm && op.nBits == 8);
                case OperandTypeEnum.IMM16: return (op.isImm && op.nBits == 16);
                case OperandTypeEnum.IMM32: return (op.isImm && op.nBits == 32);
                case OperandTypeEnum.IMM64: return (op.isImm && op.nBits == 64);

                case OperandTypeEnum.imm_imm: return op.isImm;
                case OperandTypeEnum.imm16_imm: return op.isImm;
                case OperandTypeEnum.imm_imm16: return op.isImm;
                case OperandTypeEnum.imm32_imm: return op.isImm;
                case OperandTypeEnum.imm_imm32: return op.isImm;

                case OperandTypeEnum.RM8: return ((op.isReg || op.isMem) && op.nBits == 8);
                case OperandTypeEnum.RM16: return ((op.isReg || op.isMem) && op.nBits == 16);
                case OperandTypeEnum.RM32: return ((op.isReg || op.isMem) && op.nBits == 32);
                case OperandTypeEnum.RM64: return ((op.isReg || op.isMem) && op.nBits == 64);

                case OperandTypeEnum.sbyteword: return true;
                case OperandTypeEnum.sbytedword: return true;
                case OperandTypeEnum.sbyteword16: return true;
                case OperandTypeEnum.sbytedword16: return true;
                case OperandTypeEnum.sbytedword32: return true;
                case OperandTypeEnum.sbytedword64: return true;
                case OperandTypeEnum.udword: return true;
                case OperandTypeEnum.sdword: return true;

                case OperandTypeEnum.near: return true;
                case OperandTypeEnum.far: return true;
                case OperandTypeEnum.short_ENUM: return true;
                case OperandTypeEnum.unity: return true;

                case OperandTypeEnum.mask: return true;
                case OperandTypeEnum.z: return true;

                case OperandTypeEnum.xmmreg: return (op.isReg && op.nBits == 128);
                case OperandTypeEnum.ymmreg: return (op.isReg && op.nBits == 256);
                case OperandTypeEnum.zmmreg: return (op.isReg && op.nBits == 512);

                case OperandTypeEnum.xmmreg_star: return (op.isReg && op.nBits == 128);
                case OperandTypeEnum.ymmreg_star: return (op.isReg && op.nBits == 256);
                case OperandTypeEnum.zmmreg_star: return (op.isReg && op.nBits == 512);

                case OperandTypeEnum.xmmrm128: return ((op.isReg || op.isMem) && op.nBits == 128);
                case OperandTypeEnum.ymmrm256: return ((op.isReg || op.isMem) && op.nBits == 256);
                case OperandTypeEnum.zmmrm512: return ((op.isReg || op.isMem) && op.nBits == 512);

                case OperandTypeEnum.b32: return true;
                case OperandTypeEnum.b64: return true;
                case OperandTypeEnum.reg_sreg: return true;
                case OperandTypeEnum.mem_offs: return true;
                case OperandTypeEnum.reg_creg: return true;
                case OperandTypeEnum.reg_dreg: return true;
                case OperandTypeEnum.reg_treg: return true;
                case OperandTypeEnum.reg32na: return true;
                default:
                    break;
            }
            return false;
        }

        public static String ToString(OperandTypeEnum operandType) {
            switch (operandType) {
                case OperandTypeEnum.UNKNOWN: return "unknown";
                case OperandTypeEnum.MEM: return "mem";
                case OperandTypeEnum.MEM8: return "mem8";
                case OperandTypeEnum.MEM16: return "mem16";
                case OperandTypeEnum.MEM32: return "mem32";
                case OperandTypeEnum.MEM64: return "mem64";
                case OperandTypeEnum.REG8: return "r8";
                case OperandTypeEnum.REG16: return "r16";
                case OperandTypeEnum.REG32: return "r32";
                case OperandTypeEnum.REG64: return "r64";
                case OperandTypeEnum.REG_AL: return "AL";
                case OperandTypeEnum.REG_AX: return "AX";
                case OperandTypeEnum.REG_EAX: return "EAX";
                case OperandTypeEnum.REG_RAX: return "RAX";
                case OperandTypeEnum.REG_CL: return "CL";
                case OperandTypeEnum.REG_CX: return "CX";
                case OperandTypeEnum.REG_ECX: return "ECX";
                case OperandTypeEnum.REG_RCX: return "RCX";
                case OperandTypeEnum.REG_DX: return "DX";
                case OperandTypeEnum.REG_CS: return "CS";
                case OperandTypeEnum.REG_DS: return "DS";
                case OperandTypeEnum.REG_ES: return "ES";
                case OperandTypeEnum.REG_SS: return "SS";
                case OperandTypeEnum.REG_FS: return "FS";
                case OperandTypeEnum.REG_GS: return "GS";
                case OperandTypeEnum.IMM: return "imm";
                case OperandTypeEnum.IMM8: return "imm8";
                case OperandTypeEnum.IMM16: return "imm16";
                case OperandTypeEnum.IMM32: return "imm32";
                case OperandTypeEnum.IMM64: return "imm64";
                case OperandTypeEnum.imm_imm: return "imm:imm";
                case OperandTypeEnum.imm16_imm: return "imm16:imm";
                case OperandTypeEnum.imm_imm16: return "imm:imm16";
                case OperandTypeEnum.imm32_imm: return "imm32:imm";
                case OperandTypeEnum.imm_imm32: return "imm:imm32";
                case OperandTypeEnum.RM8: return "r/m8";
                case OperandTypeEnum.RM16: return "r/m16";
                case OperandTypeEnum.RM32: return "r/m32";
                case OperandTypeEnum.RM64: return "r/m64";
                case OperandTypeEnum.sbyteword: return "sbyteword";
                case OperandTypeEnum.sbytedword: return "sbytedword";
                case OperandTypeEnum.sbyteword16: return "sbyteword16";
                case OperandTypeEnum.sbytedword16: return "sbytedword16";
                case OperandTypeEnum.sbytedword32: return "sbytedword32";
                case OperandTypeEnum.sbytedword64: return "sbytedword64";
                case OperandTypeEnum.udword: return "udword";
                case OperandTypeEnum.sdword: return "sdword";
                case OperandTypeEnum.near: return "near";
                case OperandTypeEnum.far: return "far";
                case OperandTypeEnum.short_ENUM: return "short";
                case OperandTypeEnum.unity: return "unity";
                case OperandTypeEnum.mask: return "mask";
                case OperandTypeEnum.z: return "z";
                case OperandTypeEnum.xmmreg: return "xmm";
                case OperandTypeEnum.ymmreg: return "ymm";
                case OperandTypeEnum.zmmreg: return "zmm";
                case OperandTypeEnum.xmmreg_star: return "xmm*";
                case OperandTypeEnum.ymmreg_star: return "ymm*";
                case OperandTypeEnum.zmmreg_star: return "zmm*";
                case OperandTypeEnum.xmmrm128: return "xmm/m128";
                case OperandTypeEnum.ymmrm256: return "ymm/m256";
                case OperandTypeEnum.zmmrm512: return "zmm/m512";
                case OperandTypeEnum.b32: return "b32";
                case OperandTypeEnum.b64: return "b32";
                case OperandTypeEnum.reg_sreg: return "reg_sreg";
                case OperandTypeEnum.mem_offs: return "mem_offs";
                case OperandTypeEnum.reg_creg: return "reg_creg";
                case OperandTypeEnum.reg_dreg: return "reg_dreg";
                case OperandTypeEnum.reg_treg: return "reg_treg";
                case OperandTypeEnum.reg32na: return "reg32na";

                default:
                    return "unknown";
            }
        }

        public static String getDoc(IList<OperandTypeEnum> operandType) {
            StringBuilder sb = new StringBuilder();
            foreach(OperandTypeEnum op in operandType) {
                sb.Append(SignatureElement.getDoc(op) + " or ");
            }
            sb.Length -= 4;
            return sb.ToString();
        }

        public static String getDoc(OperandTypeEnum operandType) {

            switch (operandType) {
                case OperandTypeEnum.MEM: return "memory operand";
                case OperandTypeEnum.MEM8: return "8-bits memory operand";
                case OperandTypeEnum.MEM16: return "16-bits memory operand";
                case OperandTypeEnum.MEM32: return "32-bits memory operand";
                case OperandTypeEnum.MEM64: return "64-bits memory operand";
                case OperandTypeEnum.REG8: return "8-bits register";
                case OperandTypeEnum.REG16: return "16-bits register";
                case OperandTypeEnum.REG32: return "32-bits register";
                case OperandTypeEnum.REG64: return "64-bits register";
                case OperandTypeEnum.REG_AL: return "AL register";
                case OperandTypeEnum.REG_AX: return "AX register";
                case OperandTypeEnum.REG_EAX: return "EAX register";
                case OperandTypeEnum.REG_RAX: return "RAX register";
                case OperandTypeEnum.REG_CL: return "CL register";
                case OperandTypeEnum.REG_CX: return "CX register";
                case OperandTypeEnum.REG_ECX: return "ECX register";
                case OperandTypeEnum.REG_RCX: return "RCX register";
                case OperandTypeEnum.REG_DX: return "DX register";
                case OperandTypeEnum.REG_CS: return "CS register";
                case OperandTypeEnum.REG_DS: return "DS register";
                case OperandTypeEnum.REG_ES: return "ES register";
                case OperandTypeEnum.REG_SS: return "SS register";
                case OperandTypeEnum.REG_FS: return "FS register";
                case OperandTypeEnum.REG_GS: return "GS register";
                case OperandTypeEnum.IMM: return "immediate constant";
                case OperandTypeEnum.IMM8: return "8-bits immediate constant";
                case OperandTypeEnum.IMM16: return "16-bits immediate constant";
                case OperandTypeEnum.IMM32: return "32-bits immediate constant";
                case OperandTypeEnum.IMM64: return "64-bits immediate constant";
                case OperandTypeEnum.imm_imm: return "immediate constant";
                case OperandTypeEnum.imm16_imm: return "immediate constant";
                case OperandTypeEnum.imm_imm16: return "immediate constant";
                case OperandTypeEnum.imm32_imm: return "immediate constant";
                case OperandTypeEnum.imm_imm32: return "immediate constant";
                case OperandTypeEnum.RM8: return "8-bits register or memory operand";
                case OperandTypeEnum.RM16: return "16-bits register or memory operand";
                case OperandTypeEnum.RM32: return "32-bits register or memory operand";
                case OperandTypeEnum.RM64: return "64-bits register or memory operand";
                case OperandTypeEnum.sbyteword: return "sbyteword constant";
                case OperandTypeEnum.sbytedword: return "sbytedword constant";
                case OperandTypeEnum.sbyteword16: return "sbyteword16 constant";
                case OperandTypeEnum.sbytedword16: return "sbytedword16 constant";
                case OperandTypeEnum.sbytedword32: return "sbytedword32 constant";
                case OperandTypeEnum.sbytedword64: return "sbytedword64 constant";
                case OperandTypeEnum.udword: return "udword constant";
                case OperandTypeEnum.sdword: return "sdword constant";
                case OperandTypeEnum.near: return "";
                case OperandTypeEnum.far: return "";
                case OperandTypeEnum.short_ENUM: return "";
                case OperandTypeEnum.unity: return "";
                case OperandTypeEnum.mask: return "";
                case OperandTypeEnum.z: return "";
                case OperandTypeEnum.xmmreg: return "xmm register";
                case OperandTypeEnum.ymmreg: return "ymm register";
                case OperandTypeEnum.zmmreg: return "zmm register";
                case OperandTypeEnum.xmmreg_star: return "xmm register";
                case OperandTypeEnum.ymmreg_star: return "ymm register";
                case OperandTypeEnum.zmmreg_star: return "zmm register";
                case OperandTypeEnum.xmmrm128: return "xmm register or memory operand";
                case OperandTypeEnum.ymmrm256: return "xmm register or memory operand";
                case OperandTypeEnum.zmmrm512: return "xmm register or memory operand";
                case OperandTypeEnum.b32: return "";
                case OperandTypeEnum.b64: return "";
                case OperandTypeEnum.reg_sreg: return "";
                case OperandTypeEnum.mem_offs: return "";
                case OperandTypeEnum.reg_creg: return "";
                case OperandTypeEnum.reg_dreg: return "";
                case OperandTypeEnum.reg_treg: return "";
                case OperandTypeEnum.reg32na: return "";
                default:
                    return "";
                    break;
            }

        }

        public override String ToString() {
            string[] str = new string[this.operands.Count];
            int counter1 = 0;
            foreach (IList<OperandTypeEnum> op1 in this.operands) {
                int counter2 = 0;
                string[] str2 = new string[op1.Count];
                foreach (OperandTypeEnum op2 in op1) {
                    str2[counter2] = SignatureElement.ToString(op2);
                }
                str[counter1] = string.Join("|", str2);
                counter1++;
            }
            return this.mnemonic + " " + string.Join(",", str);
        }
    }

    public class SignatureStore {
        private readonly IDictionary<Mnemonic, IList<SignatureElement>> _data;

        public SignatureStore(string fileName) {
            this._data = new Dictionary<Mnemonic, IList<SignatureElement>>();
            this.load(fileName);
        }

        private void load(string filename) {
            try {
                System.IO.StreamReader file = new System.IO.StreamReader(filename);

                string line;
                while ((line = file.ReadLine()) != null) {
                    if ((line.Length > 0) && (!line.StartsWith(";"))) {
                        string cleanedString = System.Text.RegularExpressions.Regex.Replace(line, @"\s+", " ");
                        string[] s = cleanedString.Trim().Split(' ');

                        Mnemonic mnemonic = AsmSourceTools.parseMnemonic(s[0]);
                        if (mnemonic == Mnemonic.UNKNOWN) {
                            //AsmDudeToolsStatic.Output("INFO: AsmSignatureHelpSource:load: unknown mnemonic in line" + line);
                        } else {
                            //AsmDudeToolsStatic.Output("INFO: AsmSignatureHelpSource:load: mnemonic " + mnemonic +"; s.Length="+s.Length);
                            SignatureElement se = new SignatureElement(mnemonic);

                            if (s.Length == 3) {
                                foreach (string operandStr in s[1].Split(',')) {
                                    if (operandStr.Length > 0) {
                                        //AsmDudeToolsStatic.Output("INFO: AsmSignatureHelpSource:load: operandStr " + operandStr);

                                        IList<OperandTypeEnum> operandList = new List<OperandTypeEnum>();
                                        foreach (string operand2Str in operandStr.Split('|')) {
                                            operandList.Add(parseOperandTypeEnum(operand2Str));
                                        }
                                        se.operands.Add(operandList);
                                    }
                                }
                            }

                            IList<SignatureElement> signatureElementList = null;
                            if (this._data.TryGetValue(mnemonic, out signatureElementList)) {
                                signatureElementList.Add(se);
                            } else {
                                this._data.Add(mnemonic, new List<SignatureElement> { se });
                            }
                        }
                    }
                }
                file.Close();
            } catch (FileNotFoundException) {
                MessageBox.Show("ERROR: AsmTokenTagger: could not find file \"" + filename + "\".");
            } catch (Exception e) {
                MessageBox.Show("ERROR: AsmTokenTagger: error while reading file \"" + filename + "\"." + e);
            }
        }

        public IList<SignatureElement> get(Mnemonic mnemonic) {
            IList<SignatureElement> list;
            if (this._data.TryGetValue(mnemonic, out list)) {
                return list;
            }
            return new List<SignatureElement>(0);
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<Mnemonic, IList<SignatureElement>> element in _data) {
                sb.AppendLine("Mnemonic " + element.Key + ":");
                foreach (SignatureElement sig in element.Value) {
                    sb.AppendLine("\t" + sig.ToString());
                }
            }
            return sb.ToString();
        }

        private OperandTypeEnum parseOperandTypeEnum(string str) {
            switch (str.ToUpper()) {
                case "MEM": return OperandTypeEnum.MEM;
                case "MEM8": return OperandTypeEnum.MEM8;
                case "MEM16": return OperandTypeEnum.MEM16;
                case "MEM32": return OperandTypeEnum.MEM32;
                case "MEM64": return OperandTypeEnum.MEM64;

                case "REG8": return OperandTypeEnum.REG8;
                case "REG16": return OperandTypeEnum.REG16;
                case "REG32": return OperandTypeEnum.REG32;
                case "REG64": return OperandTypeEnum.REG64;

                case "REG_AL": return OperandTypeEnum.REG_AL;
                case "REG_AX": return OperandTypeEnum.REG_AX;
                case "REG_EAX": return OperandTypeEnum.REG_EAX;
                case "REG_RAX": return OperandTypeEnum.REG_RAX;

                case "REG_CL": return OperandTypeEnum.REG_CL;
                case "REG_CX": return OperandTypeEnum.REG_CX;
                case "REG_ECX": return OperandTypeEnum.REG_ECX;
                case "REG_RCX": return OperandTypeEnum.REG_RCX;

                case "REG_DX": return OperandTypeEnum.REG_DX;

                case "REG_CS": return OperandTypeEnum.REG_CS;
                case "REG_DS": return OperandTypeEnum.REG_DS;
                case "REG_ES": return OperandTypeEnum.REG_ES;
                case "REG_SS": return OperandTypeEnum.REG_SS;
                case "REG_FS": return OperandTypeEnum.REG_FS;
                case "REG_GS": return OperandTypeEnum.REG_GS;

                case "IMM": return OperandTypeEnum.IMM;
                case "IMM8": return OperandTypeEnum.IMM8;
                case "IMM16": return OperandTypeEnum.IMM16;
                case "IMM32": return OperandTypeEnum.IMM32;
                case "IMM64": return OperandTypeEnum.IMM64;

                case "IMM:IMM": return OperandTypeEnum.imm_imm;
                case "IMM16:IMM": return OperandTypeEnum.imm16_imm;
                case "IMM:IMM16": return OperandTypeEnum.imm_imm16;
                case "IMM32:IMM": return OperandTypeEnum.imm32_imm;
                case "IMM:IMM32": return OperandTypeEnum.imm_imm32;

                case "RM8": return OperandTypeEnum.RM8;
                case "RM16": return OperandTypeEnum.RM16;
                case "RM32": return OperandTypeEnum.RM32;
                case "RM64": return OperandTypeEnum.RM64;

                case "SBYTEWORD": return OperandTypeEnum.sbyteword;
                case "SBYTEWORD16": return OperandTypeEnum.sbyteword16;

                case "SBYTEDWORD": return OperandTypeEnum.sbytedword;
                case "SBYTEDWORD16": return OperandTypeEnum.sbytedword16;
                case "SBYTEDWORD32": return OperandTypeEnum.sbytedword32;
                case "SBYTEDWORD64": return OperandTypeEnum.sbytedword64;
                case "UDWORD": return OperandTypeEnum.udword;
                case "SDWORD": return OperandTypeEnum.sdword;

                case "NEAR": return OperandTypeEnum.near;
                case "FAR": return OperandTypeEnum.far;
                case "SHORT": return OperandTypeEnum.short_ENUM;
                case "UNITY": return OperandTypeEnum.unity;

                case "MASK": return OperandTypeEnum.mask;
                case "Z": return OperandTypeEnum.z;

                case "XMMREG": return OperandTypeEnum.xmmreg;
                case "YMMREG": return OperandTypeEnum.ymmreg;
                case "ZMMREG": return OperandTypeEnum.zmmreg;
                case "XMMREG*": return OperandTypeEnum.xmmreg_star;
                case "YMMREG*": return OperandTypeEnum.ymmreg_star;
                case "ZMMREG*": return OperandTypeEnum.zmmreg_star;
                case "XMMRM128": return OperandTypeEnum.xmmrm128;
                case "YMMRM256": return OperandTypeEnum.ymmrm256;
                case "ZMMRM512": return OperandTypeEnum.zmmrm512;
                case "B32": return OperandTypeEnum.b32;
                case "B64": return OperandTypeEnum.b64;

                case "REG_SREG": return OperandTypeEnum.reg_sreg;
                case "REG_CREG": return OperandTypeEnum.reg_creg;
                case "REG_DREG": return OperandTypeEnum.reg_dreg;
                case "REG_TREG": return OperandTypeEnum.reg_treg;
                case "REG32NA": return OperandTypeEnum.reg32na;
                case "MEM_OFFS": return OperandTypeEnum.mem_offs;

                default:
                    AsmDudeToolsStatic.Output("INFO: AsmSignatureHelpSource:parseOperandTypeEnum: unknown content " + str);
                    return OperandTypeEnum.UNKNOWN;
            }
        }
    }
}
