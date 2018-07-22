import React, { Component } from 'react';
import { Layout } from 'antd';
import { Route, Switch, Redirect } from 'react-router-dom'
import HomePage from './HomePage';
import CharacterPage from './CharacterPage';
import GuildPage from './GuildPage';
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
                <Route exact path="/character/:characterId" component={CharacterPage}/>
                <Route exact path="/character/:characterId/:cursorDirection(before|after)/:cursor" component={CharacterPage}/>
                <Route exact path="/guild/:guildId" component={GuildPage}/>
                <Route exact path="/guild/:guildId/:cursorDirection(before|after)/:cursor" component={GuildPage}/>
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
