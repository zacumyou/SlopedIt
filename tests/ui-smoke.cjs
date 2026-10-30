// Focused production-bundle check. Native components/bindings are stand-ins, not a game test.
const assert = require('node:assert/strict');
const fs = require('node:fs');
const { pathToFileURL } = require('node:url');
const path = require('node:path');
const React = require('../ui/node_modules/react');
const values = { Visible: true, Enabled: true, Decal: true };
const events = [];
global.window = { React,
  'cs2/api': { bindValue: (_, key) => key, useValue: key => values[key], trigger: (...args) => events.push(args) },
  'cs2/l10n': { useLocalization: () => ({ translate: key => key }) }
};
(async () => {
  const file = path.resolve(__dirname, '../local/SlopedIt/SlopedIt.mjs');
  const source = fs.readFileSync(file, 'utf8');
  assert.match(source, /^\/\*!\s*\n \* Cities: Skylines II UI Module/);
  assert.match(source, /\* Id: SlopedIt\s*\n/);
  const module = await import(pathToFileURL(file).href);
  const snapTitle = () => null;
  const Original = props => React.createElement('section', props);
  const ToolButton = props => React.createElement('button', props);
  let Section;
  module.default({
    get: (_, name) => name === 'Loc' ? { Toolbar: { SNAPPING_TITLE: snapTitle } } : name === 'ToolButton' ? ToolButton : { button: 'native-button' },
    extend: (_, name, extend) => { assert.equal(name, 'Section'); Section = extend(Original); }
  });
  const nativeButton = React.createElement('button', { key: 'native' });
  const otherModButton = React.createElement('button', { key: 'other-mod' });
  const row = React.createElement('div', { className: 'native-row' }, nativeButton, otherModButton);
  const props = { title: React.createElement(snapTitle), children: row };
  const result = Section(props);
  const children = React.Children.toArray(result.props.children.props.children);
  assert.equal(children.length, 3);
  assert.equal(result.props.children.props.className, 'native-row');
  assert.equal(row.props.children.length, 2);
  const button = children[2].type();
  assert.equal(button.type, ToolButton);
  assert.equal(button.props.selected, true);
  assert.equal(button.props.tooltip, 'SlopedIt.DecalDescription');
  assert.match(button.props.src, /images\/SlopedIt\/stick-to-ground.svg$/);
  button.props.onSelect();
  assert.deepEqual(events, [['SlopedIt', 'Toggle']]);
  values.Enabled = false; values.Decal = false;
  assert.equal(children[2].type().props.selected, false);
  assert.equal(children[2].type().props.tooltip, 'SlopedIt.PropDescription');
  values.Visible = false;
  assert.equal(children[2].type(), null);
  const otherRow = Section({ title: React.createElement('span'), children: row });
  assert.equal(otherRow.props.children, row);
  console.log('PASS: UI module header, native snapping-row append, preserved controls, decal/prop states and toggle event.');
})().catch(error => { console.error(error); process.exitCode = 1; });
