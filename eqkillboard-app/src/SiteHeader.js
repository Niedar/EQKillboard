import React, { Component } from 'react';
import { Layout, Row, Col, Select } from 'antd';
import Search from './Search';
import { Link } from 'react-router-dom'
import { SeasonContext } from './SeasonContext';
import Route from 'react-router-dom/Route';

const { Header } = Layout;
const { Option } = Select;
class SiteHeader extends Component {
  static contextType = SeasonContext;
  render() {
    const season = this.context;
    return (
        <Header>
          <Row>
            <Col xxl={4} xl={6} lg={6} md={8} sm={6} xs={24}>
              <Link to={"/"} style={{ textDecoration: "none"}}>
                <h1 style={{color: "white"}}>EQKillboard</h1>
              </Link>
            </Col>
            <Col xxl={4} xl={0} lg={0} md={0} sm={0} xs={0}>
              <a href="https://riseofzek.com/"><img src="/rozlogo.png" style={{width: "150px"}} /></a>
            </Col>
          <Col xxl={4} xl={6} lg={0} md={0} sm={0} xs={0}>
            <Route render={({ history }) => (
              <div style={{ paddingLeft: "20px" }}>
                <Select defaultValue={season} style={{ width: 120 }} onChange={value => history.push(`/${value}/`)}>
                  <Option value={1}>Season 1</Option>
                  <Option value={2}>Season 2</Option>
                </Select>
              </div>
            )} />


            </Col>
            <Col xxl={12} xl={12} lg={18} md={16} sm={18} xs={0}>
              <Search />
            </Col>
          </Row>
        </Header>
    );
  }
}

export default SiteHeader;
