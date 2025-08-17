import ts from 'typescript';
import { registerNodeTypeHandler, registerTypeNodeForReflection } from '../reflection.js';

function handleTypePredicate(node: ts.Node, typeChecker: ts.TypeChecker): TypeScriptType {
    let typePredicateNode = node as ts.TypePredicateNode;
    
    return <TypeScriptTypePredicate> {
        kind: 'typePredicate',
        parameterName: typePredicateNode.parameterName.getText(),
        type: registerTypeNodeForReflection(typePredicateNode.type, typeChecker)
    };
}

registerNodeTypeHandler(ts.SyntaxKind.TypePredicate, handleTypePredicate);