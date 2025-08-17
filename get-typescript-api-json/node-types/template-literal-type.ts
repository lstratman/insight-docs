import ts from 'typescript';
import { registerNodeTypeHandler } from '../reflection.js';

function handleTemplateLiteralType(node: ts.Node, typeChecker: ts.TypeChecker): TypeScriptType {
    return <TypeScriptIntrinsicType> {
        kind: 'intrinsic',
        name: 'string'
    }
}

registerNodeTypeHandler(ts.SyntaxKind.TemplateLiteralType, handleTemplateLiteralType);