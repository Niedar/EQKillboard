import React, { Component } from 'react';
import KillmailsQuery from './KillmailsQuery';
import Killmails from './Killmails';
import logo from './logo.svg';
import './App.css';

class App extends Component {
  render() {
    return (
      <div className="App">
        <header className="App-header">
          <img src={logo} className="App-logo" alt="logo" />
          <h1 className="App-title">Welcome to React</h1>
        </header>
        <p className="App-intro">
          To get started, edit <code>src/App.js</code> and save to reload.
        </p>
        <KillmailsQuery>
          {({ loading, error, data}) => {
            if (loading) return 'Loading...';
            if (error) return `Error! ${error.message}`;
            
            return (
              <Killmails killmails={data.allKillmails.nodes} />
            );
          }}
        </KillmailsQuery>
      </div>
    );
  }
}

export default App;
