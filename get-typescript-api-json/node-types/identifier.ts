import * as ts from 'typescript';
import { registerNodeTypeHandler, registerSymbolForReflection } from '../reflection.js';
import NodeError from '../node-error.js';

function handleIdentifier(node: ts.Node, typeChecker: ts.TypeChecker): TypeScriptType {
    if (node.kind === ts.SyntaxKind.Identifier) {
        let typeIdentifier = node as ts.Identifier;

        if (typeIdentifier.text === 'Function') {
            return <TypeScriptIntrinsicType> {
                kind: 'intrinsic',
                name: 'Function'
            };
        }
    }

    let typeSymbol = typeChecker.getSymbolAtLocation(node);

    if (!typeSymbol) {
        throw new NodeError(`Unable to resolve the underlying type for ${node.getText()}`, node);
    }

    if (typeSymbol.declarations && typeSymbol.declarations.length > 0) {
        if (typeSymbol.declarations[0].kind === ts.SyntaxKind.TypeParameter) {
            return <TypeScriptTypeParameterType> {
                kind: 'typeParameter',
                name: typeSymbol.name
            };
        }
    }

    registerSymbolForReflection(typeSymbol, typeChecker);
        
    return <TypeScriptReferenceType> {
        kind: 'reference',
        id: typeSymbol.typeId
    };
}

registerNodeTypeHandler(ts.SyntaxKind.Identifier, handleIdentifier);
registerNodeTypeHandler(ts.SyntaxKind.QualifiedName, handleIdentifier);
registerNodeTypeHandler(ts.SyntaxKind.PropertyAccessExpression, handleIdentifier);