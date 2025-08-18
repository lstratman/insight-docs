import { nodeResolve } from '@rollup/plugin-node-resolve';
import commonjs from '@rollup/plugin-commonjs';
import json from '@rollup/plugin-json';
import copy from 'rollup-plugin-copy';

export default [
    {
        input: 'app.js',
        output: [
            {
                file: '../InsightDocs/TypeScript/Resources/get-typescript-api-json.js',
                format: 'cjs'
            }
        ],
        plugins: [
            json(),
            commonjs(),
            nodeResolve(),
            copy({
                targets: [
                    {
                        src: 'node_modules/typescript/lib/lib*.d.ts',
                        dest: '../InsightDocs/TypeScript/Resources'
                    }
                ]
            })
        ]
    }
]