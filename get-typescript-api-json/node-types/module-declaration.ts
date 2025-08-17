import * as ts from 'typescript';
import { registerNodeTypeHandler, registerSymbolForReflection, registerTypeNodeForReflection } from '../reflection.js';
import NodeError from '../node-error.js';

let nconf = require('nconf');

function handleModuleDeclaration(node: ts.Node, typeChecker: ts.TypeChecker): TypeScriptType {
    let moduleStatement = node as ts.ModuleDeclaration;
    
    if (moduleStatement.getText().startsWith('declare namespace ')) {
        if (moduleStatement.getSourceFile().fileName.endsWith('model-interfaces.d.ts')) {
            let body = moduleStatement.body;

            while (body.kind === ts.SyntaxKind.ModuleDeclaration) {
                body = (<ts.ModuleDeclaration>body).body;
            }
            
            for (let namespaceMember of (body as ts.ModuleBlock).statements) {
                if (namespaceMember.kind === ts.SyntaxKind.InterfaceDeclaration) {
                    registerSymbolForReflection(typeChecker.getSymbolAtLocation((<ts.InterfaceDeclaration>namespaceMember).name), typeChecker);
                }
            }
        }

        return null;
    }
    
    else if (moduleStatement.name.text === 'Client' || moduleStatement.name.text === 'qc/Promise' || moduleStatement.name.text.startsWith('dojox/') || (moduleStatement.name.text.startsWith('@') && nconf.get('excludePackageRoot') && !moduleStatement.name.text.endsWith('.js'))) {
        return null;
    }

    let moduleSymbol = moduleStatement.symbol || typeChecker.getSymbolAtLocation(node);

    if (moduleSymbol) {
        if (moduleSymbol.exports) {
            let assignedExportSymbol = moduleSymbol.exports.get(ts.InternalSymbolName.Default);
            let exportTypeSymbol: ts.Symbol;

            if (!assignedExportSymbol) {
                assignedExportSymbol = moduleSymbol.exports.get(ts.InternalSymbolName.ExportEquals);
            }

            if (!assignedExportSymbol && moduleSymbol.exports.size === 1) {
                exportTypeSymbol = moduleSymbol.exports.get(moduleSymbol.exports.keys().next().value);
            }

            else if (assignedExportSymbol) {
                exportTypeSymbol = typeChecker.getSymbolAtLocation((<ts.ExportAssignment>assignedExportSymbol.declarations[0]).expression);
            }

            if (!exportTypeSymbol) {
                throw new NodeError(`Unable to get the assigned export symbol for ${moduleSymbol.escapedName}`, node);
            }

            let exportType: TypeScriptType;
            let exportStyle;

            for (let exportTypeDeclaration of exportTypeSymbol.declarations) {
                if (exportTypeDeclaration.kind === ts.SyntaxKind.TypeAliasDeclaration) {
                    exportStyle = 'declaration';

                    let exportTypeAlias = exportTypeDeclaration as ts.TypeAliasDeclaration;
                    exportType = registerTypeNodeForReflection(exportTypeAlias.type, typeChecker);

                    break;
                }

                else if (exportTypeDeclaration.kind === ts.SyntaxKind.VariableDeclaration) {
                    exportStyle = 'instance';

                    let exportVariable = exportTypeDeclaration as ts.VariableDeclaration;
                    exportType = registerTypeNodeForReflection(exportVariable.type, typeChecker);

                    break;
                }

                else if (exportTypeDeclaration.kind === ts.SyntaxKind.InterfaceDeclaration) {
                    exportStyle = 'declaration';

                    let interfaceDeclaration = exportTypeDeclaration as ts.InterfaceDeclaration;

                    if (interfaceDeclaration.heritageClauses && interfaceDeclaration.heritageClauses.length === 1 && interfaceDeclaration.heritageClauses[0].types.length === 1) {
                        exportType = registerTypeNodeForReflection(interfaceDeclaration.heritageClauses[0].types[0].expression, typeChecker);
                        break;
                    }
                }

                else if (exportTypeDeclaration.kind === ts.SyntaxKind.FunctionDeclaration) {
                    let functionDeclaration = exportTypeDeclaration as ts.FunctionDeclaration;

                    if (functionDeclaration.type && functionDeclaration.type.getText().startsWith('quippe.Constructor<T & ')) {
                        exportStyle = 'declaration';
                        exportType = (<TypeScriptIntersectionType>(<TypeScriptReferenceType>registerTypeNodeForReflection(functionDeclaration.type, typeChecker)).typeArguments[0]).types[1];

                        break;
                    }

                    else {
                        let functionInterface = registerSymbolForReflection(exportTypeDeclaration.symbol, typeChecker);

                        exportStyle = 'instance';
                        exportType = <TypeScriptReferenceType> {
                            kind: 'reference',
                            id: functionInterface.id
                        };

                        break;
                    }
                }
            }

            let moduleName = moduleSymbol.escapedName.toString().replace(/'/g, '').replace(/"/g, '');

            return <TypeScriptModule> {
                kind: 'module',
                name: moduleName,
                exportStyle: exportStyle,
                exportType: exportType
            };
        }
    }

    else {
        throw new NodeError('Unable to resolve the symbol for the module', node);
    }
}

registerNodeTypeHandler(ts.SyntaxKind.ModuleDeclaration, handleModuleDeclaration);