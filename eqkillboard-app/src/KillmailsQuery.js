import React, { Component } from "react"
import { Query } from "react-apollo"
import gql from "graphql-tag"

export default class KillmailsQuery extends Component {
  render() {
    const { characterId, guildId, zoneId, children, after, before } = this.props
    var query;
    var id, first, last;

    if (before) {
      last = 50;
    } else {
      first = 50
    }

    if (characterId) {
      query = GET_CHARACTERKILLMAILS;
      id = characterId;
    } else if (guildId) {
      query = GET_GUILDKILLMAILS;
      id = guildId
    } else if (zoneId) {

    } else {
      query = GET_ALLKILLMAILS;
    }

    return (
      <Query query={query} variables={{id, first, last, after, before }}>
        {result => children(result)}
      </Query>
    )
  }
}

export const KillmailDataFragment = gql`
fragment KillmailData on Killmail {
  nodeId
  id,
  characterByVictimId {
    id,
    name
  },
  guildByVictimGuildId {
    id, 
    name
  },
  victimLevel,
  characterByAttackerId {
    id,
    name
  },
  guildByAttackerGuildId {
    id,
    name
  },
  attackerLevel,
  zoneByZoneId {
    id,
    name
  },
  killedAt  
}
`;

const GET_ALLKILLMAILS = gql`
  query allKilmails($first: Int, $last: Int, $after: Cursor, $before: Cursor){
    allKillmails(orderBy: KILLED_AT_DESC, first: $first, last: $last, after: $after, before: $before) {
      nodes {
        ...KillmailData
      },
      pageInfo {
        hasNextPage
        hasPreviousPage
        startCursor,
        endCursor
      }    
    }
  }
  ${KillmailDataFragment}
`;

const GET_CHARACTERKILLMAILS = gql`
  query characterKilmails($id: Int, $first: Int, $last: Int, $after: Cursor, $before: Cursor) {
    allKillmails(condition: {attackerId: $id}, orderBy: KILLED_AT_DESC, first: $first, last: $last, after: $after, before: $before) {
      nodes {
        ...KillmailData
      },
      pageInfo {
        hasNextPage
        hasPreviousPage
        startCursor,
        endCursor
      }    
    }
  }
  ${KillmailDataFragment}
`;

const GET_GUILDKILLMAILS = gql`
  query guildKilmails($id: Int, $first: Int, $last: Int, $after: Cursor, $before: Cursor) {
    allKillmails(condition: {attackerGuildId: $id}, orderBy: KILLED_AT_DESC, first: $first, last: $last, after: $after, before: $before) {
      nodes {
        ...KillmailData
      },
      pageInfo {
        hasNextPage
        hasPreviousPage
        startCursor,
        endCursor
      }    
    }
  }
  ${KillmailDataFragment}
`;
