import ts from 'typescript';
import { getPropertyName, registerNodeTypeHandler, registerTypeNodeForReflection } from '../reflection.js';

function handleFunctionType(node: ts.Node, typeChecker: ts.TypeChecker): TypeScriptType {
    let functionTypeNode = node as ts.FunctionTypeNode;
    let functionType: TypeScriptFunctionType = {
        kind: 'function',
        parameters: [],
        returnType: registerTypeNodeForReflection(functionTypeNode.type, typeChecker)
    };

    if (functionTypeNode.parameters) {
        for (let parameterNode of functionTypeNode.parameters) {
            functionType.parameters.push({
                kind: 'parameter',
                name: getPropertyName(parameterNode.name),
                type: registerTypeNodeForReflection(parameterNode.type, typeChecker)
            });
        }
    }

    return functionType;
}

registerNodeTypeHandler(ts.SyntaxKind.FunctionType, handleFunctionType);