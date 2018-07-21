import React, { Component } from 'react';
import { Layout } from 'antd';
import { Route, Switch, Redirect } from 'react-router-dom'
import KillmailsQuery from './KillmailsQuery';
import Killmails from './Killmails';
import HomePage from './HomePage';
import logo from './logo.svg';
import './App.css';

const { Header, Footer, Sider, Content } = Layout;
class App extends Component {
  render() {
    return (
      <div style={{ width: '58%', margin: '0 auto', background: '#fafafa' }}>
        <Layout style={{ minHeight: '100vh' }}>
          <Header><h1 style={{color: "white"}}>EQKillboard</h1></Header>
          <Layout>
            <Content>
              <Switch>
                <Route exact path="/" component={HomePage}/>
                <Route path="/:cursorDirection(before|after)/:cursor" component={HomePage}/>

                <Route>
                  <Redirect to="/" />
                </Route>
              </Switch>
            </Content>
            {/* <Sider></Sider> */}
          </Layout>
          <Footer></Footer>
        </Layout>
      </div>
    );
  }
}

export default App;
