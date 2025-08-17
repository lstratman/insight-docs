import ts from 'typescript';
import { registerNodeTypeHandler } from '../reflection.js';

function handleThisType(node: ts.Node, typeChecker: ts.TypeChecker): TypeScriptType {
    return <TypeScriptThisType> {
        kind: 'this'
    };
}

registerNodeTypeHandler(ts.SyntaxKind.ThisType, handleThisType);