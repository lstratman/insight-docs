import * as ts from 'typescript';
import * as fs from 'fs';
import * as path from 'path';
import * as nconf from 'nconf';

import { registerTypeNodeForReflection, typesLookup } from './reflection.js';
import './node-types/index.js';
import NodeError from './node-error.js';

nconf.argv({
    'outputFile': {
        demand: true,
        describe: 'Output file where the API JSON is written'
    },
    'rootDirectory': {
        demand: true,
        describe: 'Root directory where --inputFiles are found'
    },
    'inputFiles': {
        type: 'array',
        demand: true,
        describe: '.d.ts files that should be read'
    },
    'excludePackageRoot': {
        type: 'boolean',
        describe: 'Whether the package root should be excluded from analysis'
    }
});

let rootFilePath = path.join(nconf.get('rootDirectory'), 'get-typescript-metadata.d.ts');
let rootFileContents = '';

for (let inputFile of nconf.get('inputFiles')) {
    rootFileContents += `/// <reference path="${inputFile.replace(/\\/g, '/')}" />
`;
}

fs.writeFileSync(rootFilePath, rootFileContents);

try {
    let program = ts.createProgram({
        rootNames: [
            rootFilePath
        ],
        options: {
            rootDir: nconf.get('rootDirectory'),
            types: [],
            target: ts.ScriptTarget.ESNext,
            module: ts.ModuleKind.ESNext
        }
    });

    let typeChecker = program.getTypeChecker();

    let exportMetadata: TypeScriptMetadata = {
        modules: {},
        types: {}
    };

    for (let sourceFile of program.getSourceFiles()) {
        if (sourceFile.fileName.indexOf('/node_modules/@types/node/') !== -1) {
            continue;
        }

        for (let statement of sourceFile.statements) {
            if (statement.kind === ts.SyntaxKind.ModuleDeclaration) {
                let moduleMetadata = registerTypeNodeForReflection(statement, typeChecker) as TypeScriptModule;

                if (moduleMetadata) {
                    exportMetadata.modules[moduleMetadata.name] = moduleMetadata;
                }
            }
        }
    }

    for (let typeLookup of typesLookup.values()) {
        exportMetadata.types[typeLookup.typeMetadata.fullName] = typeLookup.typeMetadata;
    }

    fs.writeFileSync(nconf.get('outputFile'), JSON.stringify(exportMetadata, null, 2));
    console.log(`JSON written to ${nconf.get('outputFile')}`);
}

catch (err) {
    if (err instanceof NodeError) {
        console.error((<NodeError>err).formatMessage());
        console.error(err.stack);
    }

    else {
        console.error(err.toString());
    }

    process.exit(1);
}

finally {
    fs.unlinkSync(rootFilePath);
}