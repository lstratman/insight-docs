import * as ts from 'typescript';
import { registerNodeTypeHandler } from '../reflection.js';

function handleIntrinsicType(node: ts.Node, typeChecker: ts.TypeChecker): TypeScriptType {
    return <TypeScriptIntrinsicType> {
        kind: 'intrinsic',
        name: node.getText()
    };
}

registerNodeTypeHandler(ts.SyntaxKind.AnyKeyword, handleIntrinsicType);
registerNodeTypeHandler(ts.SyntaxKind.BooleanKeyword, handleIntrinsicType);
registerNodeTypeHandler(ts.SyntaxKind.NumberKeyword, handleIntrinsicType);
registerNodeTypeHandler(ts.SyntaxKind.VoidKeyword, handleIntrinsicType);
registerNodeTypeHandler(ts.SyntaxKind.ObjectKeyword, handleIntrinsicType);
registerNodeTypeHandler(ts.SyntaxKind.UnknownKeyword, handleIntrinsicType);
registerNodeTypeHandler(ts.SyntaxKind.UndefinedKeyword, handleIntrinsicType);
registerNodeTypeHandler(ts.SyntaxKind.NeverKeyword, handleIntrinsicType);
registerNodeTypeHandler(ts.SyntaxKind.BigIntKeyword, handleIntrinsicType);
registerNodeTypeHandler(ts.SyntaxKind.StringKeyword, handleIntrinsicType);
registerNodeTypeHandler(ts.SyntaxKind.SymbolKeyword, handleIntrinsicType);